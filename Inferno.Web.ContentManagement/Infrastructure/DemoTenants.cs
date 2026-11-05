namespace Inferno.Web.ContentManagement.Infrastructure;

/// <summary>A page and the pages under it. The HTML is the body only: the page's name is its heading.</summary>
public sealed record DemoPage(string Name, string Slug, string Html, params DemoPage[] Children);

public sealed record DemoMenuItem(string Text, string Url, params DemoMenuItem[] Children);

public sealed record DemoMenu(string Name, params DemoMenuItem[] Items);

public sealed record DemoPost(
    string Title,
    string Slug,
    string Summary,
    string Html,
    string Category,
    string[] Tags,
    int DaysAgo,
    bool IsFeatured = false);

/// <param name="IsManager">Holds every permission a tenant can hold, through the tenant's own Site Managers role. Not a host administrator.</param>
public sealed record DemoUser(string Email, string GivenName, string FamilyName, bool IsManager);

/// <summary>
/// One fake site. Each demo tenant has its own host name, theme, pages, menus, content blocks and, when
/// <see cref="EnableBlog"/> is on, blog. <see cref="ThemeId"/> names a theme in the host's Themes folder.
/// </summary>
public sealed record DemoTenant(
    string Name,
    string Url,
    string Hosts,
    string SiteName,
    string HomePageTitle,
    bool EnableBlog,
    string ThemeId,
    string ThemeAccent,
    string HomeHtml,
    string FooterHtml,
    IReadOnlyList<DemoPage> Pages,
    IReadOnlyList<DemoMenu> Menus,
    IReadOnlyList<string> Categories,
    IReadOnlyList<DemoPost> Posts,
    IReadOnlyList<DemoUser> Users);

/// <summary>The sites the installer's "sample data" option creates.</summary>
public static class DemoTenants
{
    /// <summary>
    /// The password of every demo account except the host administrator, who chose theirs. A known password is
    /// why the installer only creates these accounts when the sample data option is ticked.
    /// </summary>
    public const string UserPassword = "Demo@12345";

    /// <summary>The first tenant. The installer creates it, so the demo fills it in rather than adding it.</summary>
    public const string DefaultTenantName = "Default";

    public static IReadOnlyList<DemoTenant> All { get; } = [Corporate(), Coffee(), Studio()];

    private static DemoTenant Corporate() => new(
        Name: DefaultTenantName,
        Url: "localhost:5263",
        Hosts: "localhost:5263,localhost:7209,my-domain.com",
        SiteName: "Inferno Demo Co.",
        HomePageTitle: "Inferno Demo Co. | Software that keeps its cool",
        EnableBlog: true,
        ThemeId: "default",
        ThemeAccent: null,
        HomeHtml: """
            <section class="py-4">
              <h1 class="display-5 fw-bold">Software that keeps its cool</h1>
              <p class="lead">Inferno Demo Co. builds, runs and supports business software for teams that would rather ship than firefight.</p>
              <p><a class="btn btn-primary" href="/services">Our services</a> <a class="btn btn-outline-secondary" href="/contact">Talk to us</a></p>
            </section>
            <div class="row g-4 mb-4">
              <div class="col-md-4"><div class="card h-100"><div class="card-body"><h3 class="h5">Consulting</h3><p class="mb-0">Architecture reviews, roadmaps and honest second opinions from people who have run production systems.</p></div></div></div>
              <div class="col-md-4"><div class="card h-100"><div class="card-body"><h3 class="h5">Training</h3><p class="mb-0">Hands-on workshops for your team, built around your own code base rather than slide decks.</p></div></div></div>
              <div class="col-md-4"><div class="card h-100"><div class="card-body"><h3 class="h5">Support</h3><p class="mb-0">A named engineer, a four-hour response time and a phone number that someone actually answers.</p></div></div></div>
            </div>
            """,
        FooterHtml: """<span>12 Example Street, Springfield &middot; <a href="/contact">Contact</a> &middot; <a href="/faq">FAQ</a></span>""",
        Pages:
        [
            new DemoPage("About", "about", """
                <p>Inferno Demo Co. was founded by three engineers who were tired of watching good projects stall on avoidable problems. Today we are a team of twenty, spread over four time zones, working with clients from start-ups to public bodies.</p>
                <p>We keep our approach simple: understand the problem, make the smallest change that solves it, and leave the code better than we found it.</p>
                <ul><li>Founded in 2016</li><li>Twenty people, four time zones</li><li>Over two hundred projects delivered</li></ul>
                """,
                new DemoPage("Our team", "team", """
                    <p>Meet some of the people you will work with.</p>
                    <div class="row g-3">
                      <div class="col-md-4"><div class="card"><div class="card-body"><h3 class="h6 mb-1">Priya Raman</h3><p class="text-muted mb-1">Managing director</p><p class="mb-0">Priya has led delivery for fifteen years and still reviews every proposal.</p></div></div></div>
                      <div class="col-md-4"><div class="card"><div class="card-body"><h3 class="h6 mb-1">Tom Okafor</h3><p class="text-muted mb-1">Head of engineering</p><p class="mb-0">Tom keeps our builds green and our architecture boring, which is a compliment.</p></div></div></div>
                      <div class="col-md-4"><div class="card"><div class="card-body"><h3 class="h6 mb-1">Lena Fischer</h3><p class="text-muted mb-1">Customer success</p><p class="mb-0">Lena is the person who answers when you call, and she knows your system.</p></div></div></div>
                    </div>
                    """),
                new DemoPage("Our history", "history", """
                    <p>A short timeline of how we got here.</p>
                    <ol>
                      <li><strong>2016</strong> &ndash; Three founders, one rented desk, our first client.</li>
                      <li><strong>2018</strong> &ndash; The first hire, and the first office we did not share with a bicycle shop.</li>
                      <li><strong>2021</strong> &ndash; We went fully remote and never looked back.</li>
                      <li><strong>2024</strong> &ndash; Twenty people and our two-hundredth project.</li>
                    </ol>
                    """),
                new DemoPage("Careers", "careers", """
                    <p>We are always glad to hear from curious engineers, designers and support specialists.</p>
                    <h3 class="h5">Open roles</h3>
                    <ul><li>Senior .NET engineer (remote)</li><li>Support engineer, European hours (remote)</li><li>Technical writer (part time)</li></ul>
                    <p>Send a short note and a link to something you are proud of to <a href="mailto:careers@example.com">careers@example.com</a>.</p>
                    """)),
            new DemoPage("Services", "services", """
                <p>Three ways we can help, depending on where you are today.</p>
                <p>Every engagement starts with a free, no-obligation conversation, and every fixed-price quote is honoured even when we get it wrong.</p>
                """,
                new DemoPage("Consulting", "consulting", "<p>Architecture reviews, performance audits and technical due diligence. A typical engagement is two to six weeks and ends with a written report and a prioritised list of changes.</p><p>We have reviewed everything from hand-rolled monoliths to sprawling microservice estates, and we will tell you plainly what we find.</p>"),
                new DemoPage("Training", "training", "<p>Workshops on testing, domain modelling, secure coding and cloud operations. Groups of up to twelve, on site or online, using your own code wherever possible.</p><p>Every attendee leaves with exercises they can reuse and a one-page summary for the people who could not attend.</p>"),
                new DemoPage("Support", "support", "<p>Retained support for production systems: monitoring, patching, incident response and a monthly health check.</p><p>Plans start at ten hours a month and can be changed or cancelled at any time.</p>")),
            new DemoPage("Pricing", "pricing", """
                <p>Simple, published prices. Taxes are extra.</p>
                <div class="row g-4">
                  <div class="col-md-4"><div class="card h-100"><div class="card-body"><h3 class="h5">Starter</h3><p class="display-6">$900</p><p>Per month. Ten support hours, business-day response.</p></div></div></div>
                  <div class="col-md-4"><div class="card h-100 border-primary"><div class="card-body"><h3 class="h5">Team</h3><p class="display-6">$2,400</p><p>Per month. Forty hours, four-hour response, quarterly review.</p></div></div></div>
                  <div class="col-md-4"><div class="card h-100"><div class="card-body"><h3 class="h5">Enterprise</h3><p class="display-6">Let's talk</p><p>Custom hours, an assigned engineer and an on-call rota.</p></div></div></div>
                </div>
                """),
            new DemoPage("FAQ", "faq", """
                <h3 class="h5">Do you work with small teams?</h3><p>Yes. Most of our clients have fewer than fifty people.</p>
                <h3 class="h5">Can you take over a system someone else built?</h3><p>Often. We start with a short audit so that we both know what we are agreeing to.</p>
                <h3 class="h5">Where are you based?</h3><p>Everywhere. We are a remote company with people in four time zones.</p>
                <h3 class="h5">How quickly can we start?</h3><p>Usually within two weeks. Urgent support can start the same day.</p>
                """),
            new DemoPage("Contact", "contact", """
                <p>We would love to hear from you.</p>
                <address>Inferno Demo Co.<br />12 Example Street<br />Springfield<br /><a href="mailto:hello@example.com">hello@example.com</a><br />+1 555 0100</address>
                <p>Office hours are Monday to Friday, 9am to 5pm Eastern.</p>
                """),
        ],
        Menus:
        [
            new DemoMenu("Main",
                new DemoMenuItem("About", "/about", new DemoMenuItem("Our team", "/team"), new DemoMenuItem("Our history", "/history"), new DemoMenuItem("Careers", "/careers")),
                new DemoMenuItem("Services", "/services", new DemoMenuItem("Consulting", "/consulting"), new DemoMenuItem("Training", "/training"), new DemoMenuItem("Support", "/support")),
                new DemoMenuItem("Pricing", "/pricing"),
                new DemoMenuItem("Contact", "/contact")),
            new DemoMenu("Footer",
                new DemoMenuItem("FAQ", "/faq"),
                new DemoMenuItem("Careers", "/careers"),
                new DemoMenuItem("Privacy", "/about")),
        ],
        Categories: ["News", "Product", "Guides"],
        Posts:
        [
            new DemoPost("We have moved into a bigger office", "bigger-office", "After eight years we finally outgrew the old place, so here is the new one.",
                "<p>When we started, the whole company fitted around one table. This spring we moved into a space with room for the whole team, a proper kitchen and, at last, a meeting room with a door that closes.</p><p>Visitors are always welcome. Bring your own mug.</p>",
                "News", ["office", "company"], 4, IsFeatured: true),
            new DemoPost("What is new in version 2.0", "whats-new-in-2-0", "Faster pages, a tidier admin and a dozen small improvements.",
                "<p>Version 2.0 is the biggest release we have shipped. Pages render about forty percent faster, the admin area has been reorganised around the jobs people actually do, and every setting now has a plain-language description.</p><ul><li>Faster page rendering</li><li>Tidier navigation</li><li>Better search</li></ul>",
                "Product", ["release", "performance"], 12),
            new DemoPost("Five tips for faster pages", "five-tips-faster-pages", "Small changes that make a visible difference.",
                "<p>You do not need a rewrite to make a site feel quick. Compress your images, serve static files with long cache lifetimes, avoid blocking scripts, keep the database close to the application and measure before you optimise.</p><p>Most sites we audit get most of their win from the first two.</p>",
                "Guides", ["performance", "tutorial"], 20),
            new DemoPost("Getting started with your first site", "getting-started", "A ten-minute walk through the basics.",
                "<p>Create a page, add it to the menu, drop a block into a zone and publish. That is all it takes. This guide walks through each step with screenshots and explains where to look when something does not appear.</p>",
                "Guides", ["tutorial", "beginners"], 33),
            new DemoPost("Roadmap: what we are building this year", "roadmap", "Three themes for the next twelve months.",
                "<p>Our roadmap has three themes: make editing faster, make hosting simpler and make the whole thing easier to extend. We will share progress here every quarter.</p>",
                "News", ["roadmap", "company"], 47),
            new DemoPost("Customer spotlight: Springfield Library", "springfield-library", "How a small library put its catalogue online in a weekend.",
                "<p>The library had decades of catalogue cards and one very patient volunteer. With a spreadsheet, a few templates and an afternoon of training, the whole collection was searchable online by Monday.</p>",
                "News", ["customers"], 61),
        ],
        Users:
        [
            new DemoUser("editor@inferno.test", "Edith", "Editor", IsManager: false),
        ]);

    private static DemoTenant Coffee() => new(
        Name: "Brewhouse Coffee",
        Url: "coffee.localhost:5263",
        Hosts: "coffee.localhost:5263,coffee.localhost:7209",
        SiteName: "Brewhouse Coffee",
        HomePageTitle: "Brewhouse Coffee | Roasted small, poured slow",
        EnableBlog: false,
        ThemeId: "sunrise",
        ThemeAccent: "#b45309",
        HomeHtml: """
            <section class="py-4">
              <h1 class="display-5">Roasted small, poured slow.</h1>
              <p class="lead">Brewhouse is a neighbourhood coffee shop with its own roastery. Come for the espresso, stay for the cinnamon buns.</p>
              <p><a class="btn btn-primary" href="/menu">See the menu</a></p>
            </section>
            <div class="row g-4 mb-4">
              <div class="col-md-6"><div class="card h-100"><div class="card-body"><h3 class="h5">Opening hours</h3>
                <table class="table table-sm mb-0"><tbody>
                  <tr><td>Monday &ndash; Friday</td><td>6:30am &ndash; 5:00pm</td></tr>
                  <tr><td>Saturday</td><td>7:30am &ndash; 4:00pm</td></tr>
                  <tr><td>Sunday</td><td>8:00am &ndash; 2:00pm</td></tr>
                </tbody></table></div></div></div>
              <div class="col-md-6"><div class="card h-100"><div class="card-body"><h3 class="h5">Today's special</h3>
                <p class="mb-1"><strong>Cardamom cortado</strong> with a warm pistachio croissant.</p><p class="mb-0">Eleven dollars for the pair until the beans run out.</p></div></div></div>
            </div>
            """,
        FooterHtml: """<span>Open daily &middot; 14 Market Lane &middot; <a href="/contact">Find us</a></span>""",
        Pages:
        [
            new DemoPage("Our story", "our-story", """
                <p>Brewhouse started in 2014 as a cart at the Saturday market. We roasted in a garage, sold out by ten and spent the afternoons finding out what people wished we had brought.</p>
                <p>Today we roast three mornings a week, buy directly from farms we have visited and pay above the fair-trade floor. Every bag carries the name of the farmer who grew it.</p>
                """),
            new DemoPage("Menu", "menu", "<p>Everything is made in the shop each morning. Ask about oat, almond or soy milk at no extra charge.</p>",
                new DemoPage("Coffee", "coffee-menu", """
                    <table class="table"><thead><tr><th>Drink</th><th class="text-end">Price</th></tr></thead><tbody>
                      <tr><td>Espresso</td><td class="text-end">$3.50</td></tr>
                      <tr><td>Flat white</td><td class="text-end">$4.80</td></tr>
                      <tr><td>Cortado</td><td class="text-end">$4.20</td></tr>
                      <tr><td>Filter coffee (single origin)</td><td class="text-end">$4.50</td></tr>
                      <tr><td>Cold brew</td><td class="text-end">$5.00</td></tr>
                    </tbody></table>
                    """),
                new DemoPage("Tea and more", "tea-menu", """
                    <table class="table"><thead><tr><th>Drink</th><th class="text-end">Price</th></tr></thead><tbody>
                      <tr><td>Loose-leaf tea (pot)</td><td class="text-end">$4.50</td></tr>
                      <tr><td>Hot chocolate</td><td class="text-end">$4.80</td></tr>
                      <tr><td>Chai latte</td><td class="text-end">$5.00</td></tr>
                      <tr><td>Fresh orange juice</td><td class="text-end">$5.50</td></tr>
                    </tbody></table>
                    """),
                new DemoPage("Pastries", "pastry-menu", """
                    <table class="table"><thead><tr><th>Pastry</th><th class="text-end">Price</th></tr></thead><tbody>
                      <tr><td>Butter croissant</td><td class="text-end">$3.80</td></tr>
                      <tr><td>Pistachio croissant</td><td class="text-end">$5.20</td></tr>
                      <tr><td>Cinnamon bun</td><td class="text-end">$4.50</td></tr>
                      <tr><td>Banana bread slice</td><td class="text-end">$4.00</td></tr>
                    </tbody></table>
                    <p class="text-muted">Gluten-free options are available most days. Please ask.</p>
                    """)),
            new DemoPage("Locations", "locations", "<p>Two shops, one roastery, the same coffee.</p>",
                new DemoPage("Market Lane", "market-lane", "<address>14 Market Lane<br />Open daily, 6:30am to 5pm<br />+1 555 0142</address><p>Our original shop, next to the Saturday market. Limited seating, excellent people-watching.</p>"),
                new DemoPage("Riverside", "riverside", "<address>2 Riverside Walk<br />Open daily, 7am to 6pm<br />+1 555 0187</address><p>A bigger space with a roastery window, a long communal table and a quiet back room for working.</p>")),
            new DemoPage("Catering", "catering", """
                <p>We bring the espresso machine to you. Weddings, offices, launches and birthdays, from twenty to two hundred guests.</p>
                <ul><li>Barista and machine included</li><li>Dairy-free milks and decaf on request</li><li>Pastry platters from our bakery</li></ul>
                <p>Email <a href="mailto:events@example.com">events@example.com</a> at least a week ahead.</p>
                """),
            new DemoPage("Jobs", "jobs", """
                <p>We are hiring baristas for both shops. No experience needed if you are friendly and punctual; we will teach you the rest.</p>
                <p>Drop in with a CV or email <a href="mailto:jobs@example.com">jobs@example.com</a>.</p>
                """),
            new DemoPage("Contact", "contact", """
                <address>Brewhouse Coffee<br />14 Market Lane<br />Springfield<br /><a href="mailto:hello@example.com">hello@example.com</a><br />+1 555 0142</address>
                <p>Bean orders ship on Tuesdays and Fridays.</p>
                """),
        ],
        Menus:
        [
            new DemoMenu("Main",
                new DemoMenuItem("Our story", "/our-story"),
                new DemoMenuItem("Menu", "/menu", new DemoMenuItem("Coffee", "/coffee-menu"), new DemoMenuItem("Tea and more", "/tea-menu"), new DemoMenuItem("Pastries", "/pastry-menu")),
                new DemoMenuItem("Locations", "/locations", new DemoMenuItem("Market Lane", "/market-lane"), new DemoMenuItem("Riverside", "/riverside")),
                new DemoMenuItem("Catering", "/catering"),
                new DemoMenuItem("Contact", "/contact")),
            new DemoMenu("Footer",
                new DemoMenuItem("Jobs", "/jobs"),
                new DemoMenuItem("Catering", "/catering")),
        ],
        Categories: [],
        Posts: [],
        Users:
        [
            new DemoUser("owner@brewhouse.test", "Maya", "Brewer", IsManager: true),
        ]);

    private static DemoTenant Studio() => new(
        Name: "Pixel & Pine Studio",
        Url: "studio.localhost:5263",
        Hosts: "studio.localhost:5263,studio.localhost:7209",
        SiteName: "Pixel & Pine",
        HomePageTitle: "Pixel & Pine | Design and engineering studio",
        EnableBlog: true,
        ThemeId: "midnight",
        ThemeAccent: "#a78bfa",
        HomeHtml: """
            <section class="py-4">
              <h1 class="display-4 fw-bold">We design and build things people enjoy using.</h1>
              <p class="lead">Pixel &amp; Pine is a six-person studio making apps, websites and brands for companies that care about the details.</p>
              <p><a class="btn btn-primary" href="/work">See our work</a></p>
            </section>
            <div class="row g-4 mb-4">
              <div class="col-md-4"><div class="card h-100"><div class="card-body"><h3 class="h5">Product design</h3><p class="mb-0">Research, prototypes and interfaces that survive contact with real users.</p></div></div></div>
              <div class="col-md-4"><div class="card h-100"><div class="card-body"><h3 class="h5">Engineering</h3><p class="mb-0">Web and mobile apps built to be maintained by whoever comes next.</p></div></div></div>
              <div class="col-md-4"><div class="card h-100"><div class="card-body"><h3 class="h5">Brand</h3><p class="mb-0">Identities and design systems that stay consistent as you grow.</p></div></div></div>
            </div>
            """,
        FooterHtml: """<span>Pixel &amp; Pine Studio &middot; Hello at <a href="mailto:hello@example.com">hello@example.com</a></span>""",
        Pages:
        [
            new DemoPage("Work", "work", "<p>A few recent projects. We are happy to share more under NDA.</p>",
                new DemoPage("Aurora Bank app", "aurora-bank", "<p>A mobile banking app for a challenger bank. We redesigned onboarding, cutting sign-up from nine screens to four and lifting completion by thirty-one percent.</p><p><strong>Services:</strong> research, product design, iOS and Android engineering.</p>"),
                new DemoPage("Fieldnotes", "fieldnotes", "<p>A note-taking tool for ecologists working offline in remote places. Sync is designed around bad connections first and good ones second.</p><p><strong>Services:</strong> product design, offline-first web app.</p>"),
                new DemoPage("Kite & Co. rebrand", "kite-and-co", "<p>A complete identity for a family-run kite maker: logo, packaging, website and a small design system their own team can extend.</p><p><strong>Services:</strong> brand, packaging, web design.</p>")),
            new DemoPage("Services", "services", """
                <p>We work in small, senior teams, from a two-week prototype to a year-long build.</p>
                <ul><li>Discovery and research</li><li>Interface and interaction design</li><li>Front-end and back-end engineering</li><li>Brand identity and design systems</li></ul>
                <p>Rates are published on request and every project starts with a fixed-scope discovery phase.</p>
                """),
            new DemoPage("About", "about", """
                <p>We are six people in a converted printing works. Two designers, three engineers and one person who does a bit of everything and makes the coffee.</p>
                <p>We keep the team small on purpose. You will always work directly with the people doing the work.</p>
                """),
            new DemoPage("Contact", "contact", """
                <p>Tell us what you are making.</p>
                <address>Pixel &amp; Pine Studio<br />The Printworks, Unit 4<br />Springfield<br /><a href="mailto:hello@example.com">hello@example.com</a></address>
                """),
        ],
        Menus:
        [
            new DemoMenu("Main",
                new DemoMenuItem("Work", "/work", new DemoMenuItem("Aurora Bank app", "/aurora-bank"), new DemoMenuItem("Fieldnotes", "/fieldnotes"), new DemoMenuItem("Kite & Co. rebrand", "/kite-and-co")),
                new DemoMenuItem("Services", "/services"),
                new DemoMenuItem("About", "/about"),
                new DemoMenuItem("Contact", "/contact")),
            new DemoMenu("Footer",
                new DemoMenuItem("About", "/about"),
                new DemoMenuItem("Contact", "/contact")),
        ],
        Categories: ["Design", "Engineering", "Studio life"],
        Posts:
        [
            new DemoPost("Designing for the worst connection", "worst-connection", "Why we build offline-first and what it taught us.",
                "<p>Most of our users are on a good connection most of the time, which is exactly why the bad moments hurt. Designing for the worst case first forced us to think about what the app owes people when the network disappears: their work, their trust and a clear explanation.</p>",
                "Design", ["offline", "ux"], 3, IsFeatured: true),
            new DemoPost("A small design system that stuck", "small-design-system", "Twelve components, three rules and a lot of restraint.",
                "<p>Design systems fail when they are too big to maintain. Ours has twelve components, a tight palette and three rules. It has survived four years and two redesigns.</p>",
                "Design", ["design-systems"], 10),
            new DemoPost("Boring technology, interesting products", "boring-technology", "Our default stack and why we rarely change it.",
                "<p>We choose well-understood tools wherever we can, and keep our novelty budget for the parts of the product customers actually notice. It makes estimates better and hand-overs smoother.</p>",
                "Engineering", ["stack", "process"], 18),
            new DemoPost("What we learned from a thousand usability sessions", "usability-sessions", "The patterns we keep seeing, whatever the product.",
                "<p>After a thousand sessions the lessons repeat. People skim, they distrust long forms and they forgive slow pages less than ugly ones. Here are the seven patterns we check on every project.</p>",
                "Design", ["research", "ux"], 27),
            new DemoPost("Friday demos", "friday-demos", "Our weekly habit that replaced most of our meetings.",
                "<p>Every Friday afternoon, everyone shows something they made that week. It is unpolished on purpose. It has replaced status meetings, caught more bugs than our review process and made Mondays easier.</p>",
                "Studio life", ["culture", "process"], 40),
        ],
        Users:
        [
            new DemoUser("hello@pixelpine.test", "Sam", "Pine", IsManager: true),
        ]);
}
