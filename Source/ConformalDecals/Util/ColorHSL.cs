using System;
using System.Globalization;
using UnityEngine;

namespace ConformalDecals.Util {
    public struct ColorHSL : IEquatable<Color> {
        public float h;
        public float s;
        public float l;
        public float a;

        public ColorHSL(float h, float s = 1, float l = 0.5f, float a = 1) {
            this.h = h;
            this.s = s;
            this.l = l;
            this.a = a;
        }

        public override string ToString() {
            return $"HSLA({this.h:F3}, {this.s:F3}, {this.l:F3}, {this.a:F3})";
        }

        public string ToString(string format) {
            return
                "HSLA(" +
                $"{this.h.ToString(format, CultureInfo.InvariantCulture.NumberFormat)}, " +
                $"{this.s.ToString(format, CultureInfo.InvariantCulture.NumberFormat)}, " +
                $"{this.l.ToString(format, CultureInfo.InvariantCulture.NumberFormat)}, " +
                $"{this.a.ToString(format, CultureInfo.InvariantCulture.NumberFormat)})";
        }

        public bool Equals(ColorHSL other) {
            return (this.h.Equals(other.h) && this.s.Equals(other.s) && this.l.Equals(other.l) && this.a.Equals(other.a));
        }

        public bool Equals(Color other) {
            var rgb = HSL2RGB(this);
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
                        return this.l;
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
                        this.l = value;
                        break;
                    case 3:
                        this.a = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        public static bool operator ==(ColorHSL lhs, ColorHSL rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ColorHSL lhs, ColorHSL rhs) {
            return !(lhs == rhs);
        }

        public static implicit operator Vector4(ColorHSL c) {
            return new Vector4(c.h, c.s, c.l, c.a);
        }

        public static implicit operator ColorHSL(Vector4 v) {
            return new ColorHSL(v.x, v.y, v.z, v.w);
        }

        public static implicit operator ColorHSL(Color rgb) {
            return RGB2HSL(rgb);
        }

        public static implicit operator Color(ColorHSL hsl) {
            return HSL2RGB(hsl);
        }

        public static Color HSL2RGB(ColorHSL hsl) {
            float a = hsl.s * Mathf.Min(hsl.l, 1 - hsl.l);

            float Component(int n) {
                float k = (n + hsl.h * 12) % 12;
                return hsl.l - a * Mathf.Max(-1, Mathf.Min(k - 3, Mathf.Min(9 - k, 1)));
            }

            return new Color(Component(0), Component(8), Component(4), hsl.a);
        }

        public static ColorHSL RGB2HSL(Color rgb) {
            float h = 0;
            float s = 0;
            float l = 0;

            if (rgb.r >= rgb.g && rgb.r >= rgb.b) {
                float xMin = Mathf.Min(rgb.g, rgb.b);

                l = (rgb.r + xMin) / 2;
                s = (rgb.r - l) / Mathf.Min(l, 1 - l);

                float c = rgb.r - xMin;
                if (c > Mathf.Epsilon) h = (rgb.g - rgb.b) / (6 * c);

            }
            else if (rgb.g >= rgb.r && rgb.g >= rgb.b) {
                float xMin = Mathf.Min(rgb.r, rgb.b);

                l = (rgb.g + xMin) / 2;
                s = (rgb.g - l) / Mathf.Min(l, 1 - l);

                float c = rgb.g - xMin;
                if (c > Mathf.Epsilon) h = (2 + ((rgb.b - rgb.r) / c)) / 6;

            }
            else if (rgb.b >= rgb.r && rgb.b >= rgb.g) {
                float xMin = Mathf.Min(rgb.r, rgb.g);

                l = (rgb.b + xMin) / 2;
                s = (rgb.b - l) / Mathf.Min(l, 1 - l);

                float c = rgb.g - xMin;
                if (c > Mathf.Epsilon) h = (4 + ((rgb.r - rgb.g) / c)) / 6;

            }

            return new ColorHSL(h, s, l, rgb.a);
        }
    }
}