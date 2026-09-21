namespace Crafty.Auto.Mappers;

public interface IMapper<TData, TDto>
{
    public TData FromDto(TDto dto);
    public TDto ToDto(TData data);
}
