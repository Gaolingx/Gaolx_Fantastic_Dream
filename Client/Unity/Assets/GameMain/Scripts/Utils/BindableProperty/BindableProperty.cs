//功能：Property Helper

namespace DarkGod.Tools
{
    public class BindableProperty<T>
    {
        private T mValue;

        public System.Action<T> OnValueChanged;

        public T Value
        {
            get { return mValue; }
            set
            {
                if (!value.Equals(mValue))
                {
                    mValue = value;
                    OnValueChanged?.Invoke(mValue);
                }
            }
        }

        public BindableProperty()
        {
            this.mValue = default(T);
        }

        public BindableProperty(T value)
        {
            this.mValue = value;
        }

        public void Invoke()
        {
            OnValueChanged?.Invoke(mValue);
        }
    }
}
