// Interface IDs and layouts ported from VirtualDesktopAPI; see licenses/VirtualDesktopAPI.md.
use super::desktop::Profile;
use windows_sys::core::GUID;
pub const PROFILES: &[Profile] = &[
    Profile {
        name: "VirtualDesktopServer2022",
        manager: GUID::from_u128(0x094afe1144f24ba0976f29a97e263ee0),
        desktop: GUID::from_u128(0x62fdf88b11ca4afb8bd82296dfae49e2),
        monitor: true,
        extra_current: false,
    },
    Profile {
        name: "VirtualDesktopWin10",
        manager: GUID::from_u128(0xF31574D6B6824CDCBD561827860ABEC6),
        desktop: GUID::from_u128(0xFF72FFDDBE7E43FC9C03AD81681E88E4),
        monitor: false,
        extra_current: false,
    },
    Profile {
        name: "VirtualDesktopWin11_21H2",
        manager: GUID::from_u128(0xB2F925B95A0F4D2E9F4D2B1507593C10),
        desktop: GUID::from_u128(0x536D3495B2084CC9AE26DE8111275BF8),
        monitor: true,
        extra_current: false,
    },
    Profile {
        name: "VirtualDesktopWin11_22H2",
        manager: GUID::from_u128(0xB2F925B95A0F4D2E9F4D2B1507593C10),
        desktop: GUID::from_u128(0x536D3495B2084CC9AE26DE8111275BF8),
        monitor: true,
        extra_current: true,
    },
    Profile {
        name: "VirtualDesktopWin11_23H2",
        manager: GUID::from_u128(0xA3175F2D239C4BD28AA0EEBA8B0B138E),
        desktop: GUID::from_u128(0x3F07F4BEB107441AAF0F39D82529072C),
        monitor: false,
        extra_current: false,
    },
    Profile {
        name: "VirtualDesktopWin11_23H2_2921",
        manager: GUID::from_u128(0x53F5CA0B158F4124900C057158060B27),
        desktop: GUID::from_u128(0x3F07F4BEB107441AAF0F39D82529072C),
        monitor: false,
        extra_current: false,
    },
    Profile {
        name: "VirtualDesktopWin11_Insider",
        manager: GUID::from_u128(0x8884679816114D18946B4A67BFF58C1B),
        desktop: GUID::from_u128(0x536D3495B2084CC9AE26DE8111275BF8),
        monitor: true,
        extra_current: true,
    },
    Profile {
        name: "VirtualDesktopWin11_Insider22631",
        manager: GUID::from_u128(0x53F5CA0B158F4124900C057158060B27),
        desktop: GUID::from_u128(0x3F07F4BEB107441AAF0F39D82529072C),
        monitor: false,
        extra_current: false,
    },
    Profile {
        name: "VirtualDesktopWin11_Insider25314",
        manager: GUID::from_u128(0xA3175F2D239C4BD28AA0EEBA8B0B138E),
        desktop: GUID::from_u128(0x3F07F4BEB107441AAF0F39D82529072C),
        monitor: false,
        extra_current: false,
    },
];
