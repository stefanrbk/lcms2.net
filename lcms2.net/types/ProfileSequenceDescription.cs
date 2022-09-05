//---------------------------------------------------------------------------------
//
//  Little Color Management System
//  Copyright (c) 1998-2022 Marti Maria Saguer
//                2022      Stefan Kewatt
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//---------------------------------------------------------------------------------
//
namespace lcms2.types;

public class ProfileSequenceDescription : ICloneable, IDisposable
{
    #region Fields

    public ulong Attributes;
    public Mlu Description;
    public Signature DeviceMfg;
    public Signature DeviceModel;
    public Mlu Manufacturer;
    public Mlu Model;
    public ProfileID ProfileID;
    public Signature Technology;

    private bool _disposed;

    #endregion Fields

    #region Public Constructors

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

    #endregion Public Constructors

    #region Private Constructors

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

    #endregion Private Constructors

    #region Public Methods

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

    #endregion Public Methods
}
