using System;
using System.Text.RegularExpressions;

namespace LazyBones.Config
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public abstract class ValidatorAttribute : Attribute
    {
        public abstract bool Validate(object value);
    }

    public class MaxLengthAttribute : ValidatorAttribute
    {
        int maxLength;
        bool include;
        public MaxLengthAttribute(int maxLength)
            : this(maxLength, true)
        {
        }
        public MaxLengthAttribute(int maxLength, bool include)
        {
            this.maxLength = maxLength;
            this.include = include;
        }

        public override bool Validate(object value)
        {
            var str = value as string;
            if (str != null)
            {
                return include ? str.Length <= maxLength : str.Length < maxLength;
            }
            return false;
        }
    }

    public class RangeAttribute : ValidatorAttribute
    {
        IComparable maxValue;
        bool includeMax;
        IComparable minValue;
        bool includeMin;
        public RangeAttribute(object minValue, object maxValue)
            : this(minValue, maxValue, true, true)
        {
        }
        public RangeAttribute(object minValue, object maxValue, bool includeMin, bool includeMax)
        {
            if (Type.Equals(minValue, maxValue))
                throw new ArgumentException("参数minValue,maxValue类型不一致");

            this.minValue = minValue as IComparable;
            if (this.minValue == null)
                throw new ArgumentException("参数类型没有实现IComparable接口", "minValue");
            this.maxValue = maxValue as IComparable;
            if (this.minValue == null)
                throw new ArgumentException("参数类型没有实现IComparable接口", "maxValue");

            this.includeMin = includeMin;
            this.includeMax = includeMax;
        }
        public override bool Validate(object value)
        {
            var minResult = minValue.CompareTo(value);
            if (minResult > 0)
                return false;
            if (!includeMin && minResult == 0)
                return false;
            var maxResult = minValue.CompareTo(value);
            if (maxResult < 0)
                return false;
            if (!includeMax && maxResult == 0)
                return false;
            return true;
        }
    }

    public class MinAttribute : ValidatorAttribute
    {
        IComparable minValue;
        bool include;
        public MinAttribute(object minValue)
            : this(minValue, true)
        {
        }
        public MinAttribute(object minValue, bool include)
        {
            this.minValue = minValue as IComparable;
            if (this.minValue == null)
                throw new ArgumentException("参数类型没有实现IComparable接口", "minValue");
            this.include = include;
        }

        public override bool Validate(object value)
        {
            var result = minValue.CompareTo(value);
            return include ? result <= 0 : result < 0;
        }
    }
    public class MaxAttribute : ValidatorAttribute
    {
        IComparable maxValue;
        bool included;
        public MaxAttribute(object maxValue)
            : this(maxValue, true)
        {
        }
        public MaxAttribute(object maxValue, bool included)
        {
            this.maxValue = maxValue as IComparable;
            if (this.maxValue == null)
                throw new ArgumentException("参数类型没有实现IComparable接口", "maxValue");
            this.included = included;
        }

        public override bool Validate(object value)
        {
            var result = maxValue.CompareTo(value);
            return included ? result >= 0 : result > 0;
        }
    }
    public class RegexAttribute : ValidatorAttribute
    {
        string pattern;
        public RegexAttribute(string pattern)
        {
            this.pattern = pattern;
        }
        public override bool Validate(object value)
        {
            var str = value as string;
            if (str != null)
            {
                Regex.IsMatch(pattern, str);
            }
            return false;
        }
    }
}
