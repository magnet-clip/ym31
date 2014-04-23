﻿namespace YieldMap.Core.Application


[<AutoOpen>]
module internal Timeouts =
    // todo default timeouts
    type Timeouts = {
        load : int
        connect : int
        agent : int
        awaiter : int
    }

    let timeouts = { load = 5000; connect = 2000; agent = 1000; awaiter = 100 }

[<RequireQualifiedAccess>]
module ExternalOperations =
    open YieldMap.Core.Responses

    open YieldMap.Loader.Requests
    open YieldMap.Loader.SdkFactory
    open YieldMap.Tools.Aux
    open YieldMap.Tools.Logging

    let private logger = LogFactory.create "Operations"

    [<RequireQualifiedAccess>]
    module private Connecting = 
        let private (|TimedOut|Established|Failed|) = function
            | Some response ->
                match response with
                | Connection.Connected -> Established
                | Connection.Failed e -> Failed e
            | None -> TimedOut

        let connect (f:EikonFactory) = async { 
            let! res = f.Connect () |> Async.WithTimeout (Some timeouts.connect) 
            return
                match res with
                | TimedOut -> Success.Failure Failure.Timeout
                | Failed e -> Success.Failure <| Failure.Error e
                | Established -> Success.Ok
        }

    module Loading = 
        open YieldMap.Core.Responses

        open YieldMap.Database
        open YieldMap.Loader.MetaChains
        open YieldMap.Loader.MetaTables

        open YieldMap.Tools.Aux
        open YieldMap.Tools.Aux.Workflows.Attempt
        open YieldMap.Tools.Location
        open YieldMap.Tools.Logging

        open System.IO

        let private logger = LogFactory.create "Loading"

        do MainEntities.SetVariable("PathToTheDatabase", Location.path)
        let private cnnStr = MainEntities.GetConnectionString("TheMainEntities")

        exception DbException of Failure

        type SavingOperations = 
            abstract Backup : unit -> unit
            abstract Restore : unit -> unit
            abstract Clear : unit -> unit
            abstract NeedsReload : unit -> bool
            abstract SaveBonds : BondDescr list -> unit
            abstract SaveIssueRatings : IssueRatingData list -> unit
            abstract SaveIssuerRatings : IssuerRatingData list -> unit
            abstract SaveFrns : FrnData list -> unit

        type DbSavingOperations () = 
            // TODO OTHER OPERATIONS!!!
            interface SavingOperations with
                member x.Backup () =
                    use ctx = new MainEntities (cnnStr)
                    let path = Path.Combine(Location.path, "main.bak")
                    try
                        if File.Exists(path) then File.Delete(path)
                        let sql = sprintf "BACKUP DATABASE main TO DISK='%s'" path
                        ctx.Database.ExecuteSqlCommand(sql) |> ignore
                        if not <| File.Exists(path) then raise (DbException <| Problem "No backup file found")
                    with e ->  raise (DbException <| Error e)

                member x.Restore () = 
                    use ctx = new MainEntities (cnnStr)
                    let path = Path.Combine(Location.path, "main.bak")
                    try
                        if not <| File.Exists(path) then raise <| DbException (Problem "No restore file found")
                        let sql = sprintf "RESTORE DATABASE main FROM DISK='%s'" path
                        ctx.Database.ExecuteSqlCommand(sql) |> ignore
                        if File.Exists(path) then File.Delete(path)
                    with
                        | :? DbException -> reraise ()
                        | e -> raise <| DbException (Error e)

                member x.Clear () = ()
                member x.NeedsReload () = false
                member x.SaveBonds bonds = ()
                member x.SaveIssueRatings issue = ()
                member x.SaveIssuerRatings issuer = ()
                member x.SaveFrns frns = ()

        let private saver = DbSavingOperations () :> SavingOperations

        let (|ChainAnswer|ChainFailure|) = function
            | Choice1Of2 ch ->
                match ch with 
                | Chain.Answer a -> ChainAnswer a
                | Chain.Failed u -> ChainFailure u
            | Choice2Of2 ex -> ChainFailure ex

        let loadChains (m:ChainMetaLoader) chains = async {
            let names = chains |> List.map (fun r -> r.Ric) |> Array.ofList
            let! results = 
                chains 
                |> List.map (fun request -> m.LoadChain request |> Async.Catch)
                |> Async.Parallel
            
            let results = results |> Array.zip names

            let rics = results |> Array.choose (fun (ric, res) -> match res with ChainAnswer a -> Some (ric, a) | _ -> None)
            let fails = results |> Array.choose (fun (ric, res) -> match res with ChainFailure e -> Some (ric, e) | _ -> None)
                    
            // todo some better reporting
            do fails |> Array.iter (fun (ric, e) -> logger.WarnF "Failed to load chain %s because of %s" ric (e.ToString()))
            
            return rics, fails
        }

        type Metabuilder () = 
            member x.Bind (operation, rest) = 
                async {
                    let! res = operation
                    match res with 
                    | Meta.Answer a -> return! rest a
                    | Meta.Failed e -> return Some e
                }
            member x.Return (res : unit option) = async { return res }
            member x.Zero () = async { return None }

        let meta = Metabuilder ()

        let loadAndSaveMetadata (m:ChainMetaLoader) rics = meta {
            let! bonds = m.LoadMetadata<BondDescr> rics
            saver.SaveBonds bonds
                            
            let! frns = m.LoadMetadata<FrnData> rics
            saver.SaveFrns frns

            let! issueRatings = m.LoadMetadata<IssueRatingData> rics
            saver.SaveIssueRatings issueRatings
                            
            let! issuerRatings = m.LoadMetadata<IssuerRatingData> rics
            saver.SaveIssuerRatings issuerRatings
        }
   
        let rec reload (m:ChainMetaLoader) chains force = // todo chains are chain requests
            logger.Trace "reload ()"
            async {
                if force && saver.NeedsReload() || force then
                    try
                        saver.Backup ()
                        saver.Clear ()
                        return! load m chains
                    with :? DbException as e -> 
                        logger.ErrorEx "Load failed" e
                        return! loadFailed e
                else return Ok
            }

         and private load (m:ChainMetaLoader) requests = 
            logger.Trace "load ()"
            async {
                try
                    let! ricsByChain, fails = loadChains m requests
                    // todo throw "fails" somehow
                    let rics = ricsByChain |> Array.map snd |> Array.collect id
                    
                    let! res = loadAndSaveMetadata m rics
                    match res with 
                    | Some e -> return! loadFailed e
                    | None -> return Ok
                with :? DbException as e -> 
                    logger.ErrorEx "Load failed" e
                    return! loadFailed e
            }
        and private loadFailed (e:exn) = // todo e
            logger.Trace "loadFailed ()"
            async {
                try 
                    saver.Restore ()
                    return Failure (Problem "Failed to reload data, restored successfully")
                with e ->
                    logger.ErrorEx "Failed to reload and restore data" e
                    return Failure (Problem "Failed to reload and restore data")
            }

    // todo more advanced evaluation !!!
    let expectedLoadTime = timeouts.load
    let expectedConnectTime = timeouts.connect

    let private asSuccess timeout work = 
        work 
        >> Async.WithTimeout (Some timeout)
        >> Async.Map (function Some x -> x | None -> Failure Timeout)
    
    let load m c = Loading.reload m c |> asSuccess expectedLoadTime 
    let connect = Connecting.connect |> asSuccess expectedConnectTime

module AnotherStartup =
    open YieldMap.Core.Notifier
    open YieldMap.Core.Portfolio
    open YieldMap.Core.Responses

    open YieldMap.Loader
    open YieldMap.Loader.SdkFactory
    open YieldMap.Loader.LiveQuotes
    open YieldMap.Loader.Calendar
    open YieldMap.Loader.MetaChains
    open YieldMap.Loader.Requests

    open YieldMap.Tools.Aux
    open YieldMap.Tools.Logging

    open System
    open System.Threading

    let logger = LogFactory.create "Startup"

    type State = Started | Connected | Initialized | Closed

    type Status =
        | State of State 
        | NotResponding
        override x.ToString () = 
            match x with
            | State state -> sprintf "State %A" state
            | NotResponding -> "NotResponding"

    type Commands = 
        | Connect of State AsyncReplyChannel
        | Reload of bool * State AsyncReplyChannel
        | Close of State AsyncReplyChannel
        override x.ToString () = 
            match x with
            | Connect _ -> "Connect"
            | Reload _ -> "Reload"
            | Close _ -> "Close"

    type Startup (f:EikonFactory, c:Calendar, m:ChainMetaLoader, p:PortfolioManager)  = 
        let s = Event<_> ()

        do f.OnConnectionStatus |> Observable.add (fun state -> 
            match state with 
            | Connection.Failed e -> () 
            | Connection.Connected -> ()) // TODO ON DISCONNECT / RECONNECT DO SOMETHING (IF NECESSARY :))

        do c.NewDay |> Observable.add (fun dt -> ()) // TODO ON NEW DATE DO SOMETHING (ADD FORCE RELOAD COMMAND!!)
        
        let a = MailboxProcessor.Start (fun inbox ->

            let rec started (channel : State AsyncReplyChannel option) = 
                logger.Debug "[--> started ()]"

                async {
                    s.Trigger Started
                    match channel with Some che -> che.Reply Started | None -> ()

                    let! cmd = inbox.Receive ()
                    logger.DebugF "[Started: message %s]" (cmd.ToString())
                    match cmd with 
                    | Connect channel -> 
                        let! res = ExternalOperations.connect f
                        match res with
                        | Success.Failure f -> 
                            Notifier.notify ("Startup", f, Severity.Warn)
                            return! started (Some channel)
                        | Success.Ok -> return! connected channel
                    | Reload (force, channel) -> 
                        Notifier.notify ("Startup", Problem <| sprintf "Invalid command %s in state Started" (cmd.ToString()), Severity.Warn)
                        return! started (Some channel) 
                    | Close channel -> return close channel
                } 

            and connected (channel : State AsyncReplyChannel) = 
                logger.Debug "[--> connected ()]"

                async {
                    s.Trigger Connected
                    channel.Reply Connected

                    let! cmd = inbox.Receive ()
                    logger.DebugF "[Connected: message %s]" (cmd.ToString())
                    match cmd with 
                    | Connect channel -> 
                        Notifier.notify ("Startup", Problem <| sprintf "Invalid command %s in state Connected" (cmd.ToString()), Severity.Warn)
                        return! connected channel 
                    | Reload (_, channel) -> // ignoring force parameter on primary loading
                        logger.Debug "[Primary reload]"
                        let x = []                                                      // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        let! res = ExternalOperations.load m x true
                        match res with
                        | Success.Failure f -> 
                            Notifier.notify ("Startup", f, Severity.Warn)
                            return! connected channel
                        | Success.Ok ->  return! initialized channel
                    | Close channel -> return close channel
                }

            and initialized (channel : State AsyncReplyChannel) = 
                logger.Debug "[--> initialized ()]"

                async {
                    s.Trigger Initialized
                    channel.Reply Initialized
                    let! cmd = inbox.Receive ()
                    logger.DebugF "[Initialized: message %s]" (cmd.ToString())
                    match cmd with 
                    | Connect channel ->
                        Notifier.notify ("Startup", Problem <| sprintf "Invalid command %s in state Initialized" (cmd.ToString()), Severity.Warn)
                        return! initialized channel 
                    | Reload (force, channel) ->
                        logger.Debug "[Secondary reload]"
                        let x = []                                                      // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        let! res = ExternalOperations.load m x force
                        match res with
                        | Success.Failure f -> 
                            Notifier.notify ("Startup", f, Severity.Warn)
                            return! connected channel
                        | Success.Ok ->  return! initialized channel
                    | Close channel -> return close channel
                }

            and failed e = 
                logger.Debug "[--> failed ()]"
                Notifier.notify ("Startup", Error e, Severity.Evil)
                s.Trigger Closed

            and close channel = 
                logger.Debug "[--> closed ()]"
                s.Trigger Closed
                channel.Reply Closed

            async {
                let! res = started None |> Async.Catch
                match res with
                | Choice2Of2 e -> return failed e
                | _ -> return ()
            }
        )

        let tryCommand command timeout = async {
            let! answer = a.PostAndTryAsyncReply (command, timeout)
            match answer with
            | Some state -> return State state
            | None -> return NotResponding
        }        

        member x.StateChanged = s.Publish

        member x.Connect () = tryCommand Commands.Connect (ExternalOperations.expectedConnectTime + timeouts.agent)
        member x.Reload force = tryCommand (fun channel -> Commands.Reload (force, channel)) (ExternalOperations.expectedLoadTime + timeouts.agent)
        member x.Close () = tryCommand Commands.Close timeouts.agent