using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using lcms2.state;

namespace lcms2.plugins;

public delegate object? CreateMutexFunction(ref Context context);
public delegate void DestroyMutexFunction(ref Context context, ref object mtx);
public delegate bool LockMutexFunction(ref Context context, ref object mtx);
public delegate void UnlockMutexFunction(ref Context context, ref object mtx);

public class MutexPlugin
{

}
