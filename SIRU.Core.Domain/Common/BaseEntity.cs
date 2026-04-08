namespace SIRU.Core.Domain.Common
{
    public class BaseEntity<TKey>
    {
        public required TKey Id { get; set; }
        public required bool IsDeleted { get; set; }
    }
}
