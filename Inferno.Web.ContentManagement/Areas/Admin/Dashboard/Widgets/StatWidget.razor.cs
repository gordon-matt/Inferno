using Inferno.Web.ContentManagement.Areas.Admin.Dashboard;

namespace Inferno.Web.ContentManagement.Areas.Admin.Dashboard.Widgets;

public partial class StatWidget
{
    private string Metric => Option("metric", DashboardMetrics.PublishedPages) ?? DashboardMetrics.PublishedPages;

    private string Label => Metric switch
    {
        DashboardMetrics.PublishedPosts => "Published posts",
        DashboardMetrics.Drafts => "Drafts",
        DashboardMetrics.ScheduledPosts => "Scheduled posts",
        DashboardMetrics.Attention => "Needs attention",
        DashboardMetrics.Users => "Accounts",
        _ => "Published pages",
    };

    private string UnavailableReason
    {
        get
        {
            if (Snapshot is null || Metric == DashboardMetrics.Attention)
            {
                return null;
            }

            if (Metric == DashboardMetrics.Users)
            {
                return Snapshot.CanReadUsers ? null : "You can't view accounts";
            }

            if (Metric == DashboardMetrics.PublishedPages)
            {
                return Snapshot.CanReadPages ? null : "You can't view pages";
            }

            if (Metric is DashboardMetrics.PublishedPosts or DashboardMetrics.ScheduledPosts)
            {
                return PostReason();
            }

            if (!Snapshot.CanReadPages && PostReason() is not null)
            {
                return Snapshot.CanReadPages ? PostReason() : "You can't view pages or posts";
            }

            return null;
        }
    }

    private string Value
    {
        get
        {
            if (Snapshot is null)
            {
                return "0";
            }

            return Metric switch
            {
                DashboardMetrics.PublishedPosts => Snapshot.PublishedPosts.ToString(),
                DashboardMetrics.Drafts => (PageDrafts + PostDrafts).ToString(),
                DashboardMetrics.ScheduledPosts => Snapshot.ScheduledPosts.ToString(),
                DashboardMetrics.Attention => Snapshot.AttentionCount.ToString(),
                DashboardMetrics.Users => Snapshot.Users.ToString(),
                _ => Snapshot.PublishedPages.ToString(),
            };
        }
    }

    private string Detail
    {
        get
        {
            if (Snapshot is null || UnavailableReason is not null || Metric != DashboardMetrics.Drafts)
            {
                return Snapshot is { CanReadPages: true, ArchivedPages: > 0 } && Metric == DashboardMetrics.PublishedPages
                    ? $"{Snapshot.ArchivedPages} archived"
                    : null;
            }

            if (Snapshot.CanReadPages && PostsAvailable)
            {
                return $"{Snapshot.DraftPages} pages · {Snapshot.DraftPosts} posts";
            }

            return null;
        }
    }

    private string Href
    {
        get
        {
            if (UnavailableReason is not null)
            {
                return Metric is DashboardMetrics.PublishedPosts or DashboardMetrics.ScheduledPosts && Snapshot is { BlogEnabled: false }
                    ? DashboardLinks.Settings
                    : null;
            }

            return Metric switch
            {
                DashboardMetrics.PublishedPosts or DashboardMetrics.ScheduledPosts => DashboardLinks.Posts,
                DashboardMetrics.Drafts when PostsAvailable && !Snapshot.CanReadPages => DashboardLinks.Posts,
                DashboardMetrics.Drafts => DashboardLinks.Pages,
                DashboardMetrics.Users => DashboardLinks.Membership,
                DashboardMetrics.PublishedPages => DashboardLinks.Pages,
                _ => null,
            };
        }
    }

    private int PageDrafts => Snapshot.CanReadPages ? Snapshot.DraftPages : 0;

    private int PostDrafts => PostsAvailable ? Snapshot.DraftPosts : 0;

    private bool PostsAvailable => Snapshot.BlogEnabled && Snapshot.CanReadPosts;

    private string PostReason()
    {
        if (!Snapshot.BlogEnabled)
        {
            return "The blog is switched off";
        }

        return Snapshot.CanReadPosts ? null : "You can't view posts";
    }
}
