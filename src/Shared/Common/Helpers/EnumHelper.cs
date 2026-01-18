namespace Common.Helpers;

public static class EnumHelper
{

    #region Methods

    public static bool IsEnumValueValid<T>(int value) where T : struct, Enum => Enum.IsDefined(typeof(T), value);

    #endregion

}