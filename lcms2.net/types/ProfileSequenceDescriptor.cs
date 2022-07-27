using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lcms2.types;
public class ProfileSequenceDescriptor
{
    public Signature DeviceMfg;
    public Signature DeviceModel;
    public ulong Attributes;
    public Signature Technology;
    public ProfileID ProfileID;
    public Mlu Manufacturer;
    public Mlu Model;
    public Mlu Description;
}
