﻿using System.Collections.Generic;
using System.Linq;

namespace YieldMap.Transitive.Tools {
    internal class Set<T> {
        public static readonly Set<T> Empty = new Set<T>();
        private readonly HashSet<T> _data;

        public Set(IEnumerable<T> data) {
            _data = new HashSet<T>(data);
        }

        public Set() {
            _data = new HashSet<T>();
        }

        public IEnumerable<T> ToEnumerable() {
            return new HashSet<T>(_data);
        }

        public T[] ToArray() {
            return _data.ToArray();
        }

        public Set<T> Add(IEnumerable<T> items) {
            return new Set<T>(_data.Add(items));
        }

        public Set<T> Add(T item) {
            var data = new HashSet<T>(_data) {item};
            return new Set<T>(data);
        }
        
        public Set<T> Union(Set<T> another) {
            var res = new HashSet<T>(_data);
            res.UnionWith(another._data);
            return new Set<T>(res);
        }

        public Set<T> Intersect(Set<T> another) {
            var res = new HashSet<T>(_data);
            res.IntersectWith(another._data);
            return new Set<T>(res);
        }

        public Set<T> Subtract(Set<T> another) {
            var res = new HashSet<T>(_data);
            res = res.Remove(another._data);
            return new Set<T>(res);
        }

        public Set<T> SymmetricDiff(Set<T> another) {
            var res = new HashSet<T>(_data);
            res.SymmetricExceptWith(another._data);
            return new Set<T>(res);
        }

        public static Set<T> operator +(Set<T> x, Set<T> y) {
            if (x == null && y == null)
                return null;
            if (x == null)
                return y;
            return y == null ? x : x.Union(y);
        }

        public static Set<T> operator +(Set<T> x, T t) {
            if (x != null) 
                return x.Add(t);

            var res = new Set<T>();
            res = res.Add(new[] {t});
            return res;
        }

        public static Set<T> operator -(Set<T> x, Set<T> y) {
            if (x == null)
                return null;
            return y == null ? x : x.Subtract(y);
        }

        public static Set<T> operator *(Set<T> x, Set<T> y) {
            if (x == null || y == null)
                return null;
            return x.Intersect(y);
        }

        protected bool Equals(Set<T> other) {
            return Equals(_data, other._data);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((Set<T>)obj);
        }

        public override int GetHashCode() {
            return (_data != null ? _data.GetHashCode() : 0);
        }

        public static bool Equals(Set<T> left, Set<T> right) {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;

            if (!ReferenceEquals(left, null) && !ReferenceEquals(right, null))
                return left.Equals(right);

            return
                (ReferenceEquals(left, null) && ReferenceEquals(right, Empty)) ||
                (ReferenceEquals(right, null) && ReferenceEquals(left, Empty));
        }

        public static bool operator ==(Set<T> left, Set<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(Set<T> left, Set<T> right) {
            return !Equals(left, right);
        }

        public bool Contains(T id) {
            return _data.Contains(id);
        }
    }
}