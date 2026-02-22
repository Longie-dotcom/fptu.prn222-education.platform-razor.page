using DataAccessLayer.Interface;
using DataAccessLayer.Persistence;
using Domain.OrderManagement.Aggregate;
using Domain.OrderManagement.Enum;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Implementation
{
    public class OrderRepository :
        GenericRepository<Order>,
        IOrderRepository
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        public OrderRepository(EducationPlatformDBContext context) : base(context) { }

        #region Methods
        public async Task<IEnumerable<Order>> GetOrders(
            string? status,
            int pageIndex,
            int pageSize,
            Guid? teacherId)
        {
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            IQueryable<Order> query = context.Orders
                .AsNoTracking();

            // ---- Status filter ----
            if (!string.IsNullOrWhiteSpace(status)
                && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(o => o.Status == parsedStatus);
            }

            // ---- Teacher filter (via Courses table) ----
            if (teacherId.HasValue)
            {
                query = query.Where(o =>
                    context.Courses.Any(c =>
                        c.CourseID == o.CourseID &&
                        c.TeacherID == teacherId.Value));
            }

            // ---- Sorting + paging ----
            return await query
                .OrderByDescending(o => o.PaidAt ?? DateTime.MinValue)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByOrderCode(
            long orderCode)
        {
            return await context.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }
        #endregion
    }
}
