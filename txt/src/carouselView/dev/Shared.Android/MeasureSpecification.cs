using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Platform
{
    internal struct MeasureSpecification
    {
        [Flags]
        internal enum MeasureSpecificationType
        {
            Unspecified = 0,
            Exactly = 0x1 << 31,
            AtMost = 0x1 << 32,
            Mask = Exactly | AtMost
        }

        public static explicit operator MeasureSpecification(int measureSpecification)
        {
            return new MeasureSpecification(measureSpecification);
        }
        public static implicit operator int(MeasureSpecification measureSpecification)
        {
            return measureSpecification.Encode();
        }

        internal MeasureSpecification(int measureSpecification)
        {
            Value = measureSpecification & (int)~MeasureSpecificationType.Mask;
            Type = (MeasureSpecificationType)(measureSpecification & (int)MeasureSpecificationType.Mask);
        }
        internal MeasureSpecification(int value, MeasureSpecificationType measureSpecification)
        {
            Value = value;
            Type = measureSpecification;
        }

        internal int Value { get; }
        internal MeasureSpecificationType Type { get; }
        internal int Encode() => Value | (int)Type;

        public override string ToString()
        {
            return string.Format("{0} {1}", Value, Type);
        }
    }
}
