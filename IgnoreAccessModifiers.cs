// Veilheim
// a Valheim mod
//
// File:    IgnoreAccessModifiers.cs
// Project: Veilheim

using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
