/*
 * Copyright 2024 (c) Leia Inc.  All rights reserved.
 *
 * NOTICE:  All information contained herein is, and remains
 * the property of Leia Inc. and its suppliers, if any.  The
 * intellectual and technical concepts contained herein are
 * proprietary to Leia Inc. and its suppliers and may be covered
 * by U.S. and Foreign Patents, patents in process, and are
 * protected by trade secret or copyright law.  Dissemination of
 * this information or reproduction of this materials strictly
 * forbidden unless prior written permission is obtained from
 * Leia Inc.
 */
#if UNITY_ANDROID
using System;
using System.Runtime.InteropServices;

namespace Leia {
namespace HeadTracking {
    using FaceIdx = UInt64;

    static class Constants
    {
        public const int kRightEyeIdx = 0;
        public const int kLeftEyeIdx = 1;
        public const int kNumEyes = 2;

        public const int kMaxNumFaces = 3;
    }
    public enum Status
    {
        Success = 0,
        ErrorInvalidInstance,
        ErrorUnknown,
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MovingPoint
    {
        public Leia.Vector3 pos;
        public Leia.Vector3 vel;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Face
    {
        public MovingPoint point;
        public Leia.Vector3 angle;
        public FaceIdx rawFaceIndex;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RawFace
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Constants.kNumEyes)]
        public Leia.Vector3[] eyePoints;
        public Leia.Vector3 trackingPoint;
        public FaceIdx detectedFaceIndex;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DetectedFace
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Eye
        {
            public Leia.Vector2d imageCoord;
            public float depth;
        }

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Constants.kNumEyes)]
        public Eye[] eyes;
        public Leia.Vector3 posePosition;
        public Leia.Vector3 poseAngle;
        public UInt32 id;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Result
    {
        public Int32 numFaces;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Constants.kMaxNumFaces)]
        public Face[] faces;

        public Leia.Timestamp timestamp;

        public Int32 jumpFlag;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FrameProfiling {
        public Int64 cameraExposureTime;
        public Int64 faceDetectorStartTime;
        public Int64 faceDetectorEndTime;
        public Int64 apiTimestamp;
    }

    public class NativeCallFailedException : Exception
    {
        public Status Status;
        public NativeCallFailedException(Status status) : base()
        {
            Status = status;
        }
        public override string Message
        {
            get => Status.ToString();
        }
    }
    static class Utils
    {
        static public void HandleNativeCall(int nativeStatus)
        {
            var status = (Status)nativeStatus;
            if (status != Status.Success)
            {
                throw new NativeCallFailedException(status);
            }
        }
    }
    public struct RawFaces
    {
        public int numFaces;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Constants.kMaxNumFaces)]
        public RawFace[] faces;
    }
    public struct DetectedFaces
    {
        public int numFaces;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = Constants.kMaxNumFaces)]
        public DetectedFace[] faces;
    }
    public class Frame : IDisposable
    {
        public Frame(IntPtr unmanagedHandle)
        {
            _unmanagedHandle = unmanagedHandle;
        }
        public void Dispose()
        {
            if (_unmanagedHandle != IntPtr.Zero)
            {
                leia_headtracking_frame_release(_unmanagedHandle);
                _unmanagedHandle = IntPtr.Zero;
            }
        }
        public void GetTrackingResult(out Result trackingResult)
        {
            Utils.HandleNativeCall(leia_headtracking_frame_get_tracking_result(_unmanagedHandle, out trackingResult));
        }
        public void GetDetectedFaces(out DetectedFaces detectedFaces)
        {
            Utils.HandleNativeCall(leia_headtracking_frame_get_detected_faces(_unmanagedHandle, out detectedFaces));
        }
        public void GetRawFaces(out RawFaces rawFaces)
        {
            Utils.HandleNativeCall(leia_headtracking_frame_get_raw_faces(_unmanagedHandle, out rawFaces));
        }

        private IntPtr _unmanagedHandle;

        [DllImport(Leia.Constants.SDK_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern Int32 leia_headtracking_frame_get_tracking_result(IntPtr frame, out Result result);
        [DllImport(Leia.Constants.SDK_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern Int32 leia_headtracking_frame_get_raw_faces(IntPtr frame, out RawFaces raw);
        [DllImport(Leia.Constants.SDK_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern Int32 leia_headtracking_frame_get_detected_faces(IntPtr frame, out DetectedFaces detected);
        [DllImport(Leia.Constants.SDK_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern Int32 leia_headtracking_frame_get_profiling(IntPtr frame, out FrameProfiling frameProfiling);
        [DllImport(Leia.Constants.SDK_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void leia_headtracking_frame_release(IntPtr frame);
    }
} // namespace HeadTracking
} // namespace Leia
#endif