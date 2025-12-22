namespace glox.Records;
public readonly record struct NumberType(string Name)
{
    public static readonly NumberType Integer = new("int");
    public static readonly NumberType Double  = new("double");

    public override string ToString() => Name;
}