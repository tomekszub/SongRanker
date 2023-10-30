public abstract class UniqueIDProperty<Tself>
{
    protected int _id;

    public int SetId(int id) => _id = id;

    public abstract bool IsEqual(Tself property);
    public abstract bool IsValid();
}
