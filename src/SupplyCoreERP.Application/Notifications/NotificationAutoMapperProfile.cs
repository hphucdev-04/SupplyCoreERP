using AutoMapper;
using SupplyCoreERP.Notifications.Dtos;

namespace SupplyCoreERP.Notifications;

public class NotificationApplicationAutoMapperProfile : Profile
{
    public NotificationApplicationAutoMapperProfile()
    {
        CreateMap<Notification, NotificationDto>()
            .ForMember(d => d.IsRead, opt => opt.Ignore());
    }
}
