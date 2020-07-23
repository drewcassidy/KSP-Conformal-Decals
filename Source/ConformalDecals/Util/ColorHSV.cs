using System;
using System.Globalization;
using UnityEngine;

namespace ConformalDecals.Util {
    public struct ColorHSV : IEquatable<Color> {
        public float h;
        public float s;
        public float v;
        public float a;

        public ColorHSV(float h, float s = 1, float v = 1, float a = 1) {
            this.h = h;
            this.s = s;
            this.v = v;
            this.a = a;
        }

        public override string ToString() {
            return $"HSVA({this.h:F3}, {this.s:F3}, {this.v:F3}, {this.a:F3})";
        }

        public string ToString(string format) {
            return
                "HSVA(" +
                $"{this.h.ToString(format, CultureInfo.InvariantCulture.NumberFormat)}, " +
                $"{this.s.ToString(format, CultureInfo.InvariantCulture.NumberFormat)}, " +
                $"{this.v.ToString(format, CultureInfo.InvariantCulture.NumberFormat)}, " +
                $"{this.a.ToString(format, CultureInfo.InvariantCulture.NumberFormat)})";
        }

        public bool Equals(ColorHSL other) {
            return (this.h.Equals(other.h) && this.s.Equals(other.s) && this.v.Equals(other.l) && this.a.Equals(other.a));
        }

        public bool Equals(Color other) {
            var rgb = HSV2RGB(this);
            return rgb.Equals(other);
        }

        public override bool Equals(object obj) {
            if (obj is ColorHSL otherHSL) return Equals(otherHSL);
            if (obj is Color otherRGB) return Equals(otherRGB);

            return false;
        }

        public override int GetHashCode() {
            return ((Vector4) this).GetHashCode();
        }

        public float this[int index] {
            get {
                switch (index) {
                    case 0:
                        return this.h;
                    case 1:
                        return this.s;
                    case 2:
                        return this.v;
                    case 3:
                        return this.a;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
            set {
                switch (index) {
                    case 0:
                        this.h = value;
                        break;
                    case 1:
                        this.s = value;
                        break;
                    case 2:
                        this.v = value;
                        break;
                    case 3:
                        this.a = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        public static bool operator ==(ColorHSV lhs, ColorHSV rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ColorHSV lhs, ColorHSV rhs) {
            return !(lhs == rhs);
        }

        public static implicit operator Vector4(ColorHSV c) {
            return new Vector4(c.h, c.s, c.v, c.a);
        }

        public static implicit operator ColorHSV(Vector4 v) {
            return new ColorHSV(v.x, v.y, v.z, v.w);
        }

        public static implicit operator ColorHSV(Color rgb) {
            return RGB2HSV(rgb);
        }

        public static implicit operator Color(ColorHSV hsv) {
            return HSV2RGB(hsv);
        }

        public static Color HSV2RGB(ColorHSV hsv) {
            var rgb = Color.HSVToRGB(hsv.h, hsv.s, hsv.v, false);
            rgb.a = hsv.a;
            return rgb;
        }

        public static ColorHSV RGB2HSV(Color rgb) {
            var hsv = new ColorHSV {a = rgb.a};
            Color.RGBToHSV(rgb, out hsv.h, out hsv.s, out hsv.v);
            return hsv; 
        }
    }
}