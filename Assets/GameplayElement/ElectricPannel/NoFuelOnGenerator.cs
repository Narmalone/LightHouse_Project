public class NoFuelOnGenerator : ItemBase
{
    private string forPlayer = "Generator is Empty !";
    public override string Name { get => forPlayer; set => forPlayer = value; }
}
