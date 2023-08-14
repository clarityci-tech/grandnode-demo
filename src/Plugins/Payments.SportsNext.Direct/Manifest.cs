using System;
using Grand.Infrastructure;
using Grand.Infrastructure.Plugins;
using Payments.SportsNext.Direct;

[assembly: PluginInfo(
    FriendlyName = "NBC Sports Next Payment Service (Direct)",
    Group = "Payment methods",
    SystemName = SNDirectDefaults.ProviderSystemName,
    SupportedVersion = GrandVersion.SupportedPluginVersion,
    Author = "NBC Sports Next - FinTech",
    Version = "2.1.1"
)]