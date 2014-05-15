﻿namespace YieldMap.Tests.Unit

open System
open System.IO

open NUnit.Framework
open FsUnit

module StartupTest = 
    open YieldMap.Loader.Calendar
    open YieldMap.Loader.MetaChains
    open YieldMap.Loader.SdkFactory

    open YieldMap.Core.Application
    open YieldMap.Core.Application.Operations
    open YieldMap.Core.Application.Startup
    open YieldMap.Core.Notifier

    open YieldMap.Database

    open YieldMap.Tools.Location
    open YieldMap.Tools.Logging

    open System.Data.Entity
    open System.Linq

    let logger = LogFactory.create "StartupTest"
    
    [<TestCase>]
    let ``Simple workflow with empty database`` () =
        let dt = DateTime(2014,3,4)
                
        let zzz = {
            Factory = MockFactory()
            TodayFix = dt
            Loader = MockChainMeta dt
            Calendar = MockCalendar dt
        }

        let x = Startup zzz

        x.StateChanged |> Observable.add (fun state -> logger.InfoF " => %A" state)
        Notifier.notification |> Observable.add (fun (state, fail, severity) -> 
            let m =
                match severity with
                | Evil -> logger.ErrorF
                | Warn -> logger.TraceF
                | Note -> logger.InfoF

            m "MSG: @%A %s" state <| fail.ToString()
        )

        let command cmd func state = 
            logger.InfoF "===> %s " cmd
            let res = func () |> Async.RunSynchronously  
            logger.InfoF " <=== %s : %s " cmd (res.ToString())
            res |> should be (equal state)

        command "Connect" x.Connect (State Connected)
        command "Reload" (fun () -> x.Reload true) (State Initialized)
        command "Connect" x.Connect (State Initialized)
        command "Reload" (fun () -> x.Reload (true, 1000000000)) (State Initialized) // NOW IT IS INFINITE HENCE I CAN TEST IT
        command "Close" x.Close (State Closed)
        command "Close" x.Close NotResponding
        command "Connect" x.Connect NotResponding


    MainEntities.SetVariable("PathToTheDatabase", Location.path)
    let cnnStr = MainEntities.GetConnectionString("TheMainEntities")

    let rec clear (ctx:MainEntities) (table : 'a DbSet) =
        let items = query { for x in table do select x }
        let item = items.ToList().FirstOrDefault()
        if item <> null then
            table.Remove item |> ignore
            ctx.SaveChanges () |> ignore
            clear ctx table
                
                
    let initDb chains = 
        use eraser = new Eraser ()

        eraser.DeleteChains ()
        eraser.DeleteBonds ()
        eraser.DeleteFeeds ()
        
        use ctx = new MainEntities (DbConn.ConnectionString)
        let idn = ctx.Feeds.Add <| Feed(Name = "Q")
        ctx.SaveChanges () |> ignore

        chains |> Array.iter (fun name -> ctx.Chains.Add <| Chain(Name = name, Feed = idn, Params = "") |> ignore)
        ctx.SaveChanges () |> ignore

    let checkData numChains dt =
        let cnt (table : 'a DbSet) = 
            query { for x in table do 
                    select x
                    count }
        
        use ctx = new MainEntities(cnnStr)
        cnt ctx.Feeds |> should be (equal 1)
        cnt ctx.Chains |> should be (equal numChains)

        let ch = query { for ch in ctx.Chains do 
                         select ch } // todo not exactly one ))

        ch |> Seq.iter (fun ch -> ch.Expanded.Value |> should be (equal dt))

    type x = {
        chains : string array
        date : DateTime
    }

    let boo = seq {
//        yield { chains = [|"0#RUAER=MM"; "0#RUCORP=MM"; "0#RUTSY=MM"|]; date = DateTime(2014,5,8) }
        yield { chains = [|"0#RUAER=MM"|]; date = DateTime(2014,5,14) }
//        yield { chains = [|"0#RUCORP=MM"; "0#RUTSY=MM"; "0#RUAER=MM"|]; date = DateTime(2014,5,14) } // ; "0#RUEUROS=" // ; "0#RUCORP=MM"; "0#RUTSY=MM"
    }

    // todo full cleanup!!!
    // todo reload overnight

    [<Test>]
    [<TestCaseSource("boo")>]
    let ``Startup with one chain`` xxx =
        let { date = dt; chains = prms } = xxx
        
        globalThreshold := LoggingLevel.Info

        initDb prms

        let x = Startup {
            Factory = MockFactory()
            TodayFix = dt
            Loader = MockChainMeta dt
            Calendar = MockCalendar dt
        }

        x.StateChanged |> Observable.add (fun state -> logger.InfoF " => %A" state)
        Notifier.notification |> Observable.add (fun (state, fail, severity) -> 
            let m =
                match severity with
                | Evil -> logger.ErrorF
                | Warn -> logger.TraceF
                | Note -> logger.InfoF

            m "MSG: @%A %s" state (fail.ToString())
        )

        let command cmd func state = 
            logger.InfoF "===> %s " cmd
            let res = func () |> Async.RunSynchronously  
            logger.InfoF " <=== %s : %s " cmd (res.ToString())
            res |> should be (equal state)

        command "Connect" x.Connect (State Connected)
        command "Reload" (fun () -> x.Reload (true, 100000000)) (State Initialized)
        command "Connect" x.Connect (State Initialized)

        logger.Error " =============== SECOND RELOAD ====================="
        command "Reload" (fun () -> x.Reload true) (State Initialized)
        command "Close" x.Close (State Closed)
        command "Close" x.Close NotResponding
        command "Connect" x.Connect NotResponding

        checkData (Array.length prms) dt