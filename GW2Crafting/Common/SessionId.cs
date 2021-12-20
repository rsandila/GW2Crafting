namespace GW2Crafting.Common
{
    public static class SessionId
    {
        public static Guid GetSessionId(HttpContext ctx)
        {
            var id = ctx.Session.GetString("Id");
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var result))
            {
                return Guid.Empty;
            }
            return result;
        }
        public static void SetSessionId(HttpContext ctx, Guid id)
        {
            ctx.Session.SetString("Id", id.ToString());
        }

        internal static void ResetSession(HttpContext ctx)
        {
            ctx.Session.Clear();
        }
    }
}
