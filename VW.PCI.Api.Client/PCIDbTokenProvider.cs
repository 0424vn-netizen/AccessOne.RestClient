using VW.Api.RestClient.Models;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;

namespace VW.PCI.Api.Client
{
    /// <summary>
    /// Token provider lưu vào DB.
    /// Phù hợp khi chạy multi-instance (nhiều server dùng chung token).
    /// Token sống qua app restart.
    ///
    /// Cách dùng: kế thừa class này, implement 3 method abstract với DB framework của bạn.
    ///
    /// Ví dụ:
    ///   public class MyPCITokenProvider : PCIDbTokenProvider
    ///   {
    ///       private readonly IDbContext _db;
    ///
    ///       public MyPCITokenProvider(AuthTokenRequest credentials, ILogger logger, IDbContext db)
    ///           : base(credentials, logger) { _db = db; }
    ///
    ///       protected override PCITokenInfo GetTokenFromDB()
    ///           => _db.PCITokens.FirstOrDefault()?.ToPCITokenInfo();
    ///
    ///       protected override void SaveTokenToDB(PCITokenInfo token)
    ///       {
    ///           var existing = _db.PCITokens.FirstOrDefault();
    ///           if (existing != null) { existing.Update(token); }
    ///           else { _db.PCITokens.Add(token.ToEntity()); }
    ///           _db.SaveChanges();
    ///       }
    ///
    ///       protected override void InvalidateTokenInDB()
    ///       {
    ///           var existing = _db.PCITokens.FirstOrDefault();
    ///           if (existing != null) { existing.ExpireAt = DateTime.MinValue; _db.SaveChanges(); }
    ///       }
    ///   }
    /// </summary>
    public abstract class PCIDbTokenProvider : PCITokenProvider
    {
        protected PCIDbTokenProvider(AuthTokenRequest credentials, ILogger logger)
            : base(credentials, logger) { }

        protected override abstract PCITokenInfo GetTokenFromDB();
        protected override abstract void SaveTokenToDB(PCITokenInfo token);
        protected override abstract void InvalidateTokenInDB();
    }
}
