using System;

namespace Services.Core.Tags.Interface.Events
{
    public record ContentChangeEvent(
        DateTime Timestamp,
        Guid Id        
    );
}
