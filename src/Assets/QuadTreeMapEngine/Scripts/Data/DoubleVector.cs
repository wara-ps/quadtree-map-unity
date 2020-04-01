using System;
using System.Xml.Serialization;
using UnityEngine;

namespace QuadTreeMapEngine.Data
{
    public abstract class DoubleVector<T> where T : DoubleVector<T>
    {
        public abstract double Magnitude { get; }
        public new abstract string ToString();

        public static T operator +(DoubleVector<T> lhs, DoubleVector<T> rhs)
        {
            return (T)lhs.Add(rhs);
        }

        public static T operator -(DoubleVector<T> lhs, DoubleVector<T> rhs)
        {
            return (T)lhs.Sub(rhs);
        }

        public static T operator *(double factor, DoubleVector<T> v)
        {
            return (T)v.Scale(factor);
        }

        protected abstract DoubleVector<T> Add(DoubleVector<T> other);
        protected abstract DoubleVector<T> Sub(DoubleVector<T> other);
        protected abstract DoubleVector<T> Scale(double factor);
    }

    public class Double2 : DoubleVector<Double2>
    {
        [XmlAttribute]
        public double X { get; set; }

        [XmlAttribute]
        public double Y { get; set; }

        public override double Magnitude => Math.Sqrt(X * X + Y * Y);

        public Double2() { }

        public Double2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X:#.#}, {Y:#.#})";
        }

        protected override DoubleVector<Double2> Add(DoubleVector<Double2> other)
        {
            var tmp = (Double2)other;
            return new Double2(X + tmp.X, Y + tmp.Y);
        }

        protected override DoubleVector<Double2> Sub(DoubleVector<Double2> other)
        {
            var tmp = (Double2)other;
            return new Double2(X - tmp.X, Y - tmp.Y);
        }

        protected override DoubleVector<Double2> Scale(double factor)
        {
            return new Double2(factor * X, factor * Y);
        }

        public static implicit operator Vector2(Double2 d)
        {
            return new Vector2((float)d.X, (float)d.Y);
        }
    }

    public class Double3 : DoubleVector<Double3>
    {
        [XmlAttribute]
        public double X { get; set; }

        [XmlAttribute]
        public double Y { get; set; }

        [XmlAttribute]
        public double Z { get; set; }

        public Double3() { }

        public Double3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString()
        {
            return $"({X:#.#}, {Y:#.#}, {Z:#.#})";
        }

        protected override DoubleVector<Double3> Add(DoubleVector<Double3> other)
        {
            var tmp = (Double3)other;
            return new Double3(X + tmp.X, Y + tmp.Y, Z + tmp.Z);
        }

        protected override DoubleVector<Double3> Sub(DoubleVector<Double3> other)
        {
            var tmp = (Double3)other;
            return new Double3(X - tmp.X, Y - tmp.Y, Z - tmp.Z);
        }

        protected override DoubleVector<Double3> Scale(double factor)
        {
            return new Double3(factor * X, factor * Y, factor * Z);
        }

        public static implicit operator Vector3(Double3 d)
        {
            return new Vector3((float)d.X, (float)d.Y, (float)d.Z);
        }
    }
}
