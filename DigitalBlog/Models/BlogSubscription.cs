using System;
using System.Collections.Generic;

namespace DigitalBlog.Models;

public partial class BlogSubscription
{
    public int Subid { get; set; }

    public decimal SubAmount { get; set; }

    public short UserId { get; set; }

    public long Bid { get; set; }

    public virtual Blog BidNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
