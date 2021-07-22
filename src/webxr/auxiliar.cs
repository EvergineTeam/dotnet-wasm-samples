using System;
// using System.Globalization;
// using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 12)]
public struct Vector3
{
    [FieldOffset(0)]
    public float X;

    [FieldOffset(4)]
    public float Y;

    [FieldOffset(8)]
    public float Z;
}

public struct Quaternion
{
    public float X;
    public float Y;
    public float Z;
    public float W;
}

public struct ViewPose
{
    public Vector3 Position;
    public Quaternion Orientation;
}

public struct Matrix4x4
{
    public float M44;
    public float M43;
    public float M42;
    public float M41;
    public float M34;
    public float M33;
    public float M32;
    public float M31;
    public float M23;
    public float M22;
    public float M21;
    public float M14;
    public float M13;
    public float M12;
    public float M11;
    public float M24;

    public Matrix4x4(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
    {
        this.M11 = m11;
        this.M12 = m12;
        this.M13 = m13;
        this.M14 = m14;
        this.M21 = m21;
        this.M22 = m22;
        this.M23 = m23;
        this.M24 = m24;
        this.M31 = m31;
        this.M32 = m32;
        this.M33 = m33;
        this.M34 = m34;
        this.M41 = m41;
        this.M42 = m42;
        this.M43 = m43;
        this.M44 = m44;
    }
}

public struct ViewProperties
{
    public ViewPose Pose;
    public Matrix4x4 Projection;
}