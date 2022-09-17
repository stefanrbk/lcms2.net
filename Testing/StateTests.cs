using lcms2.state;

namespace lcms2.testbed;

public static class StateTests
{
    #region Public Methods

    public static bool TestAdaptationStateState()
    {
        var old1 = State.SetAdaptationState(null, -1);

        var c1 = State.CreateStateContainer(null, null);

        State.SetAdaptationState(c1, 0.7);

        var c2 = State.DuplicateStateContainer(c1, null);
        var c3 = State.DuplicateStateContainer(c2, null);

        var rc = IsGoodDouble("Adaption state", State.SetAdaptationState(c3, -1), 0.7, 0.001);

        State.DeleteStateContainer(c1);
        State.DeleteStateContainer(c2);
        State.DeleteStateContainer(c3);

        var old2 = State.SetAdaptationState(null, -1);

        if (old1 != old2)
            return Fail("Adaptation state has changed");

        return rc;
    }

    public static bool TestAlarmCodes()
    {
        var codes = new ushort[] { 0x0000, 0x1111, 0x2222, 0x3333, 0x4444, 0x5555, 0x6666, 0x7777, 0x8888, 0x9999, 0xaaaa, 0xbbbb, 0xcccc, 0xdddd, 0xeeee, 0xffff };

        var c1 = State.CreateStateContainer(null, null);

        State.SetAlarmCodes(c1, (ushort[])codes.Clone());
        var c2 = State.DuplicateStateContainer(c1, null);
        var c3 = State.DuplicateStateContainer(c2, null);

        var values = State.GetAlarmCodes(c3);

        
        for (var i = 0; i < 16; i++)
        {
            if (values[i] != codes[i])
                return Fail($"Bad alarm code {values[i]} != {codes[i]}");
        }

        return true;
    }

    public static bool TestSimpleState()
    {
        object a = 1;
        object b = 32;

        var c1 = State.CreateStateContainer(null, a);

        // Let's check duplication
        var c2 = State.DuplicateStateContainer(c1, null);
        var c3 = State.DuplicateStateContainer(c2, null);

        // User data should have been propagated
        var rc = State.GetUserData(c3) == a;
        
        State.DeleteStateContainer(c1);
        State.DeleteStateContainer(c2);
        State.DeleteStateContainer(c3);

        if (!rc)
            return Fail("Creation of user data failed");

        c1 = State.CreateStateContainer(null, a);

        c2 = State.DuplicateStateContainer(c1, null);
        c3 = State.DuplicateStateContainer(c2, b);

        // New user data should be applied to c3
        rc = State.GetUserData(c1) == a &&
             State.GetUserData(c2) == a &&
             State.GetUserData(c3) == b;

        State.DeleteStateContainer(c1);
        State.DeleteStateContainer(c2);
        State.DeleteStateContainer(c3);
        
        if (!rc)
            Fail("Modification of user data failed");

        return true;
    }

    #endregion Public Methods
}
