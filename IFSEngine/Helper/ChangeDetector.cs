using System;
using System.Collections.Generic;
using System.Text;

namespace IFSEngine.Helper
{
    public class ChangeDetector<T>
    {
        private T value;
        public event EventHandler<T> ValueChanged;
        public static implicit operator T(ChangeDetector<T> prop)
        {
            return prop.value;
        }

        /// <returns>value changed</returns>
        public bool Update(T value)
        {
            if (this.value == null || !this.value.Equals(value))
            {
                this.value = value;
                ValueChanged?.Invoke(this, value);
                return true;
            }
            return false;
        }

        public ChangeDetector() { }

        public ChangeDetector(T defaulValue)
        {
            value = defaulValue;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
