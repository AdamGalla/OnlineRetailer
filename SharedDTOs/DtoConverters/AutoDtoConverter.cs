using AutoMapper;

namespace SharedDTOs.DtoConverters;

public static class DTOConverter<T, U>
{
    private readonly static MapperConfiguration config = new(cfg =>
    {
        cfg.CreateMap<T, U>();
    });
    private readonly static Mapper mapper = new(config);

    //  OUTPUT          <FROM> <TO>     <SOURCE>
    // var As = DtoConverter<B, A>.FromList(Bs);
    // var B = DtoConverter<A, B>.From(A);
    // var A = DtoConverter<B, A>.From(B);
    public static U From(T sourceObject) => mapper.Map<T, U>(sourceObject);
    public static IEnumerable<U> FromList(IEnumerable<T> sourceList) => sourceList.ToList().Select(obj => From(obj));
}
