﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftRenderer
{
    public struct Matrix
    {
        public Matrix(float[] values = null)
        {
            Values = new float[16];
            if (values == null)
            {
                return;
            }
            for (var i = 0; i < Values.Length; i++)
            {
                Values[i] = values[i];
            }
        }

        public static Matrix Identity => new Matrix(new float[16] {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        });

        public float[] Values { get; }

        public static Matrix LookAtLH(Vector eye, Vector target, Vector up)
        {
            var axisZ = (target - eye).Normalize();
            var axisX = up.Cross(axisZ).Normalize();
            var axisY = axisZ.Cross(axisX).Normalize();

            var eyeX = -axisX.Dot(eye);
            var eyeY = -axisY.Dot(eye);
            var eyeZ = -axisZ.Dot(eye);

            return new Matrix(new[] {
                axisX.x, axisY.x, axisZ.x, 0,
                axisX.y, axisY.y, axisZ.y, 0,
                axisX.z, axisY.z, axisZ.z, 0,
                eyeX, eyeY, eyeZ, 1
            });
        }

        public static Matrix Zero()
        {
            return new Matrix(new float[16]);
        }

        public static bool operator !=(Matrix a, Matrix b)
        {
            return !(a == b);
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            var values = new float[16];
            for (var index = 0; index < 16; index++)
            {
                var i = index / 4;
                var j = index % 4;
                values[index] =
                    a.Values[i * 4] * b.Values[j] +
                    a.Values[i * 4 + 1] * b.Values[1 * 4 + j] +
                    a.Values[i * 4 + 2] * b.Values[2 * 4 + j] +
                    a.Values[i * 4 + 3] * b.Values[3 * 4 + j];
            }
            return new Matrix(values);
        }

        public static bool operator ==(Matrix a, Matrix b)
        {
            return !a.Values.Where((t, i) => Math.Abs(t - b.Values[i]) > float.MinValue).Any();
        }

        public static Matrix PerspectiveFovLH(float fieldOfView, float aspect, float znear, float zfar)
        {
            var height = 1 / (float)Math.Tan(fieldOfView / 2);
            var width = height / aspect;
            return new Matrix(new[] {
                width, 0, 0, 0,
                0, height, 0, 0,
                0, 0, zfar/(zfar - znear), 1,
                0, 0, znear*zfar/(znear - zfar), 0
            });
        }

        public static Matrix Rotation(Vector r)
        {
            var x = RotationX(r.x);
            var y = RotationY(r.y);
            var z = RotationZ(r.z);
            return z * x * y;
        }

        public static Matrix RotationAngle(Vector r)
        {
            var x = r.x * MathUtility.Angle2Rad;
            var y = r.y * MathUtility.Angle2Rad;
            var z = r.z * MathUtility.Angle2Rad;
            return Rotation(new Vector(x,y,z));
        }

        public static Matrix RotationX(float rad)
        {
            var s = (float)Math.Sin(rad);
            var c = (float)Math.Cos(rad);
            var values = new[] {
            1, 0,  0, 0,
            0, c,  s, 0,
            0, -s, c, 0,
            0, 0,  0, 1
        };
            return new Matrix(values);
        }

        public static Matrix RotationXAngle(float angle)
        {
            float rad = angle * MathUtility.Angle2Rad;
            return RotationX(rad);
        }

        public static Matrix RotationY(float angle)
        {
            var s = (float)Math.Sin(angle);
            var c = (float)Math.Cos(angle);
            var values = new[] {
            c, 0, -s, 0,
            0, 1, 0,  0,
            s, 0, c,  0,
            0, 0, 0,  1
        };
            return new Matrix(values);
        }

        public static Matrix RotationYAngle(float angle)
        {
            float rad = angle * MathUtility.Angle2Rad;
            return RotationY(rad);
        }

        public static Matrix RotationZ(float angle)
        {
            var s = (float)Math.Sin(angle);
            var c = (float)Math.Cos(angle);
            var values = new[] {
            c,  s, 0, 0,
            -s, c, 0, 0,
            0,  0, 1, 0,
            0,  0, 0, 1,
        };
            return new Matrix(values);
        }

        public static Matrix RotationZAngle(float angle)
        {
            float rad = angle * MathUtility.Angle2Rad;
            return RotationZ(rad);
        }

        public static Matrix Scale(Vector s)
        {
            var values = new[] {
                s.x, 0, 0, 0,
                0, s.y, 0, 0,
                0, 0, s.z, 0,
                0, 0, 0, 1
            };
            return new Matrix(values);
        }

        public static Matrix Translation(Vector t)
        {
            var values = new[] {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                t.x, t.y, t.z, 1
            };
            return new Matrix(values);
        }

        public static Matrix PerspectiveFov(float fovY, float aspect, float zNear, float zFar)
        {
            float cot = 1 / (float)Math.Tan(fovY * MathUtility.Angle2Rad * 0.5f);
            var values = new[] {
                cot/aspect,  0,  0,  0,
                0, cot, 0, 0,
                0, 0, zFar/ (zFar - zNear), 1,
                0, 0, zNear * zFar /(zNear - zFar), 0
            };
            return new Matrix(values);
        }

        public bool Equals(Matrix other)
        {
            return Equals(Values, other.Values);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Matrix && Equals((Matrix)obj);
        }

        public override int GetHashCode()
        {
            return Values?.GetHashCode() ?? 0;
        }

        public Vector ApplyTransfer(Vector v)
        {
            var x = v.x * Values[0 * 4 + 0] + v.y * Values[1 * 4 + 0] + v.z * Values[2 * 4 + 0] + Values[3 * 4 + 0];
            var y = v.x * Values[0 * 4 + 1] + v.y * Values[1 * 4 + 1] + v.z * Values[2 * 4 + 1] + Values[3 * 4 + 1];
            var z = v.x * Values[0 * 4 + 2] + v.y * Values[1 * 4 + 2] + v.z * Values[2 * 4 + 2] + Values[3 * 4 + 2];
            var w = v.x * Values[0 * 4 + 3] + v.y * Values[1 * 4 + 3] + v.z * Values[2 * 4 + 3] + Values[3 * 4 + 3];
            return new Vector(x, y, z, w);
        }
    }
}
