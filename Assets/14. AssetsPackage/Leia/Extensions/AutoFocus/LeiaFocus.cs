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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaUnity
{
    [DefaultExecutionOrder(3)]
    public abstract class LeiaFocus : MonoBehaviour
    {
        protected LeiaDisplay leiaDisplay;

        [Tooltip("The range of allowable depth factor values for the Leia Display. Can be used to prevent depth from becoming too large or too small.")]
        [SerializeField] MinMaxPair _depthFactorRange = new MinMaxPair(0.01f, 0, "Min", 10f, 100, "Max");
        [Tooltip("The range of allowable focal distances for the Leia Display. Can be used to prevent the focal plane from becoming too far away or too close to the camera.")]
        [SerializeField] MinMaxPair _focalDistanceRange = new MinMaxPair(1, 1.0e-5f, "Min", 1000, 1E+07f, "Max");

        [Tooltip("After the LeiaDisplay's baseline is determined by the auto depth algorithm, and is clamped between the min and max baseline, it will be scaled by this amount. A value of 1 is the default, recommended value. This setting can be used to implement a settings slider which allows a user of the app to adjust how much depth it has.")]
        [SerializeField, Range(0.01f, 1)]
        private float _depthScale = 0.1f;

        [Tooltip("Minimum percentage the computed focal distance must change by before the target value is updated. If the focus is jittery, try increasing this value.")]
        [SerializeField, Range(0, 1)] private float focalDistanceChangeThreshold = .01f;
        [Tooltip("Minimum percentage the computed depth factor must change by before the target value is updated. If the Leia Display bounds are jittery, try increasing this value.")]
        [SerializeField, Range(0, 1)] private float depthFactorChangeThreshold = .05f;

        protected float targetFocalDistance;
        private float targetFocalDistancePrev;

        private float targetDepthFactor;
        private float targetDepthFactorPrev;

        [SerializeField] private bool _setFocalDistance = true;
        [SerializeField] private bool _setDepthFactor = true;

        public enum FocusSpaceType { Camera, World }
        [Tooltip("To have the focal plane stay in the same place relative to the camera when the camera moves forward / backward on it's local z-axis, use the \"Camera\" focus space. To have the focal plane stay in the same place relative to the world when the camera moves forward / backward on it's local z-axis, use \"World\" focus space.")]
        [SerializeField] private FocusSpaceType FocusSpace;

        public bool SetFocalDistance

        {
            get
            {
                return _setFocalDistance;
            }
            set
            {
                _setFocalDistance = value;
            }
        }

        public bool SetDepthFactor
        {
            get
            {
                return _setDepthFactor;
            }
            set
            {
                _setDepthFactor = value;
            }
        }

        public float DepthScale
        {
            get
            {
                return _depthScale;
            }
            set
            {
                _depthScale = Mathf.Clamp(value, 0, float.MaxValue);
            }
        }

        public float MinDepthFactor
        {
            get
            {
                return _depthFactorRange.min;
            }
            set
            {
                _depthFactorRange.min = Mathf.Clamp(value, 0, float.MaxValue);
            }
        }

        public float MaxDepthFactor
        {
            get
            {
                return _depthFactorRange.max;
            }
            set
            {
                _depthFactorRange.max = Mathf.Clamp(value, 0, float.MaxValue);
            }
        }

        public float MinFocalDistance
        {
            get
            {
                return _focalDistanceRange.min;
            }
            set
            {
                _focalDistanceRange.min = Mathf.Clamp(value, 0, float.MaxValue);
            }
        }

        public float MaxFocalDistance
        {
            get
            {
                return _focalDistanceRange.max;
            }
            set
            {
                _focalDistanceRange.max = Mathf.Clamp(value, 0, float.MaxValue);
            }
        }

        const float idealFramerate = 60f;

        [Tooltip("Speed at which the focal distance changes from its current value"), Range(.01f, 1f), SerializeField]
        private float _focusSpeed = 0.5f;

        public float FocusSpeed
        {
            get
            {
                return _focusSpeed;
            }
            set
            {
                _focusSpeed = value;
            }
        }

        [Tooltip("Speed at which the depth factor changes from its current value"), Range(.01f, 1f), SerializeField]
        private float _depthFocusSpeed = 0.1f;

        public float DepthFocusSpeed
        {
            get
            {
                return _depthFocusSpeed;
            }
            set
            {
                _depthFocusSpeed = value;
            }
        }

        [Tooltip("Offset the LeiaDisplay focal distance from its computed optimal value by this amount. This can be useful to bring objects more to the foreground to make them pop out more, or push them more into the background so that (for example) UI can be drawn over them."), SerializeField] private float _focusOffset;
        public float FocusOffset
        {
            get
            {
                return _focusOffset;
            }
            set
            {
                _focusOffset = value;
            }
        }

        private RunningFloatAverage targetFocalDistanceHistory;
        const int targetFocalDistanceHistoryLength = 5;
        RunningFloatAverage targetDepthFactorHistory;
        const int targetDepthFactorHistoryLength = 15;

        private Vector3 localPositionPrev;

        protected virtual void OnEnable()
        {
            localPositionPrev = transform.parent.localPosition;
            leiaDisplay = GetComponent<LeiaDisplay>();
            targetFocalDistanceHistory = new RunningFloatAverage(targetFocalDistanceHistoryLength);
            targetDepthFactorHistory = new RunningFloatAverage(targetDepthFactorHistoryLength);
            targetFocalDistanceHistory.AddSample(leiaDisplay.FocalDistance);
            targetDepthFactorHistory.AddSample(leiaDisplay.DepthFactor);
        }

        protected void SetTargetFocalDistance(float newTargetFocalDistance)
        {
            targetFocalDistanceHistory.AddSample(newTargetFocalDistance);
        }

        protected void SetTargetDepthFactor(float newTargetDepthFactor)
        {
            targetDepthFactorHistory.AddSample(newTargetDepthFactor);
        }

        void UpdateFocalDistance()
        {
            float target = targetFocalDistanceHistory.Average;

            //if new target changed by more than convergenceChangeThreshold %, then update target
            if (Mathf.Abs(target - targetFocalDistance) > focalDistanceChangeThreshold * targetFocalDistance
                    && Mathf.Abs(target - targetFocalDistancePrev) > focalDistanceChangeThreshold * targetFocalDistance)
            {
                targetFocalDistancePrev = targetFocalDistance;
                targetFocalDistance = target;
            }

            float newFocalDistance = CalculateNewFocalDistance();

            //Clamp convergence between min and max values
            newFocalDistance = Mathf.Clamp(newFocalDistance, this._focalDistanceRange.min, this._focalDistanceRange.max);

            //Prevent focus offset from causing the convergence plane to go behind the camera
            if (FocusOffset + targetFocalDistance < 1.0e-5f)
            {
                FocusOffset = -targetFocalDistance + 1.0e-5f;
                newFocalDistance = CalculateNewFocalDistance();
            }
            leiaDisplay.FocalDistance = newFocalDistance;
        }

        private float CalculateNewFocalDistance()
        {
            float frameSpeed = Mathf.Min(
                FocusSpeed * Time.deltaTime * idealFramerate,
                1f
            );

            float newFocalDistance = leiaDisplay.FocalDistance +
                ((targetFocalDistance + FocusOffset) - leiaDisplay.FocalDistance)
                * frameSpeed;

            //Prevent convergence from being set to closer than the camera's near clip plane
            newFocalDistance = Mathf.Max(leiaDisplay.HeadCamera.nearClipPlane, newFocalDistance);

            return newFocalDistance;
        }

        void UpdateDepthFactor()
        {
            float target = targetDepthFactorHistory.Average;

            //if new target changed by more than baselineChangeThreshold %, then update target
            if (Mathf.Abs(target - targetDepthFactor) > depthFactorChangeThreshold * targetDepthFactor
                && Mathf.Abs(target - targetDepthFactorPrev) > depthFactorChangeThreshold * targetDepthFactor)
            {

                targetDepthFactorPrev = targetDepthFactor;

                targetDepthFactor = Mathf.Clamp(
                    target,
                    _depthFactorRange.min,
                    _depthFactorRange.max
                    );
            }

            float frameSpeed = Mathf.Min(
                DepthFocusSpeed * Time.deltaTime * idealFramerate,
                1f
            );

            leiaDisplay.DepthFactor +=
                (targetDepthFactor * DepthScale - leiaDisplay.DepthFactor)
                * frameSpeed;

            leiaDisplay.DepthFactor = Mathf.Clamp(
                leiaDisplay.DepthFactor,
                MinDepthFactor,
                MaxDepthFactor
            );
        }

        protected virtual void LateUpdate()
        {
            if (leiaDisplay.mode == LeiaDisplay.ControlMode.DisplayDriven)
            {
                return;
            }

            if (this.SetDepthFactor)
            {
                UpdateDepthFactor();
            }
            if (this.SetFocalDistance)
            {
                UpdateFocalDistance();
            }

            if (this.FocusSpace == FocusSpaceType.World)
            {
                Vector3 cameraMovement = transform.parent.localPosition - localPositionPrev;

                leiaDisplay.FocalDistance -= cameraMovement.z;

                localPositionPrev = transform.parent.localPosition;
            }
        }

        public void AddOffset(float offset)
        {
            targetFocalDistanceHistory.AddOffset(offset);
        }
    }
}

public class RunningFloatAverage
{
    private float _average;
    public float Average
    {
        get
        {
            return _average;
        }
    }
    private int _maxSamplesCount;
    public int maxSamplesCount
    {
        get
        {
            return _maxSamplesCount;
        }
        private set
        {
            _maxSamplesCount = Mathf.Max(value, 1);
        }
    }

    private readonly IndexedQueue<float> sampleValues;

    public int Count
    {
        get
        {
            return sampleValues.Count;
        }
    }

    public RunningFloatAverage(int maxSamplesCount)
    {
        this.maxSamplesCount = maxSamplesCount;
        sampleValues = new IndexedQueue<float>(maxSamplesCount);
    }

    public void AddSample(float value)
    {
        sampleValues.Enqueue(value);

        if (sampleValues.Count > maxSamplesCount)
        {
            sampleValues.Dequeue();
        }

        _average = ComputeAverage();
    }

    private float ComputeAverage()
    {
        float count = sampleValues.Count;
        float sum = 0;

        for (int i = 0; i < count; i++)
        {
            sum += sampleValues[i];
        }


        float average = sum / count;

        return average;
    }

    public void AddOffset(float offset)
    {
        float count = sampleValues.Count;

        for (int i = 0; i < count; i++)
        {
            sampleValues[i] += offset;
        }
    }

    public void Reset()
    {
        sampleValues.Reset();
    }
}

public class IndexedQueue<T>
{
    private int currentPosition;
    private int count;
    readonly private T[] values;

    public int Count
    {
        get
        {
            return count;
        }
    }

    public IndexedQueue(int startCount)
    {
        values = new T[startCount];
    }

    public void Enqueue(T value)
    {
        values[currentPosition] = value;
        currentPosition++;
        if (currentPosition == values.Length)
        {
            currentPosition = 0;
        }
        count = Mathf.Min(values.Length, count + 1);
    }

    public T Dequeue()
    {
        if (count > 0)
        {
            T dequeuedValue = values[currentPosition];
            currentPosition--;
            if (currentPosition == -1)
            {
                currentPosition = values.Length - 1;
            }
            count--;
            return dequeuedValue;
        }
        return default(T);
    }

    public void Reset()
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = default(T);
        }
        currentPosition = 0;
    }
    int BoundReadPosition(int position)
    {
        if (position >= values.Length)
        {
            position -= values.Length;
        }

        return position;
    }

    public T this[int position]
    {
        get
        {
            return values[BoundReadPosition(position)];
        }
        set
        {
            values[BoundReadPosition(position)] = value;
        }
    }
}