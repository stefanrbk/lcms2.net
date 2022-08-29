using lcms2.state;

namespace lcms2.types;

public class ProfileSequenceDescription: ICloneable, IDisposable
{
    public ulong Attributes;
    public Mlu Description;
    public Signature DeviceMfg;
    public Signature DeviceModel;
    public Mlu Manufacturer;
    public Mlu Model;
    public ProfileID ProfileID;
    public Signature Technology;

    private bool _disposed;

    public ProfileSequenceDescription(object? context, Signature deviceMfg, Signature deviceModel, ulong attributes, Signature technology, ProfileID profileID)
    {
        DeviceMfg = deviceMfg;
        DeviceModel = deviceModel;
        Attributes = attributes;
        Technology = technology;
        ProfileID = profileID;
        Manufacturer = new Mlu(context);
        Model = new Mlu(context);
        Description = new Mlu(context);
        _disposed = false;
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
        _disposed = false;
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
        if (!_disposed)
        {
            Manufacturer?.Dispose();
            Model?.Dispose();
            Description?.Dispose();

            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
