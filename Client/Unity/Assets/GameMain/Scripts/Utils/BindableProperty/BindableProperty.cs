//¹¦ÄÜ£ºProperty Helper

public class BindableProperty<T>
{
    private T mValue = default(T);

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

    public void Invoke()
    {
        OnValueChanged?.Invoke(mValue);
    }
}
