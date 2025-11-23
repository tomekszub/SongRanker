public abstract class UniqueIDProperty<Tself>
{
    protected int _id = -1;

    public int SetId(int id) => _id = id;

    public abstract bool IsEqual(Tself property);
    public abstract bool IsValid();
}
