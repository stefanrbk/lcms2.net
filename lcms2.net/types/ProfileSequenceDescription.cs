using lcms2.state;

namespace lcms2.types;
public class ProfileSequenceDescription : ICloneable, IDisposable
{
    public Signature DeviceMfg;
    public Signature DeviceModel;
    public ulong Attributes;
    public Signature Technology;
    public ProfileID ProfileID;
    public Mlu Manufacturer;
    public Mlu Model;
    public Mlu Description;
    private bool disposed;

    public ProfileSequenceDescription(Context? context, Signature deviceMfg, Signature deviceModel, ulong attributes, Signature technology, ProfileID profileID)
    {
        DeviceMfg = deviceMfg;
        DeviceModel = deviceModel;
        Attributes = attributes;
        Technology = technology;
        ProfileID = profileID;
        Manufacturer = new Mlu(context);
        Model = new Mlu(context);
        Description = new Mlu(context);
        disposed = false;
    }

    private ProfileSequenceDescription(Signature deviceMfg, Signature deviceModel, ulong attributes, Signature technology, ProfileID profileID, Mlu manufacturer, Mlu model, Mlu description)
    {
        DeviceMfg = deviceMfg;
        DeviceModel = deviceModel;
        Attributes = attributes;
        Technology = technology;
        ProfileID = profileID;
        Manufacturer = manufacturer;
        Model = model;
        Description = description;
        disposed = false;
    }

    public object Clone() =>
        new ProfileSequenceDescription(
            DeviceMfg,
            DeviceModel,
            Attributes,
            Technology,
            ProfileID,
            (Mlu)Manufacturer.Clone(),
            (Mlu)Model.Clone(),
            (Mlu)Description.Clone());

    public void Dispose()
    {
        if (!disposed) {

            Manufacturer?.Dispose();
            Model?.Dispose();
            Description?.Dispose();

            disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
