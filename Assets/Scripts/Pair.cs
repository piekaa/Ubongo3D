public class Pair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public Pair(TKey key, TValue value)
    {
        this.Key = key;
        this.Value = value;
    }
}