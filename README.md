# SignPadPicker

사인패드 모듈을 선택적으로 사용할 수 있는 플러그인 모듈.

## Framework

- .NET Framework 4.0

## Libraries

### SignPadPicker module loader

```
└─ SignPadPicker.dll
```

### SignPadPicker.ScreenSignPadAdaptor plug-in library

- 사인패드 기기 없이 마우스로 그릴 수 있는 대체 화면 기능 제공.

```
└─ SignPadPicker.ScreenSignPadAdaptor.dll
```

### SignPadPicker.KisSignPadAdaptor plug-in library

- Vendor: KIS정보통신
- Client
  - 단국대병원

```
│  KisDongleDll.dll
└─ SignPadPicker.KisSignPadAdaptor.dll
```

### SignPadPicker.KscatSignPadAdaptor plug-in library

- Vendor: KSNET
- Client
  - 분당제생병원
  - 성남시의료원

```
│  ksnetcomm.dll
└─ SignPadPicker.KscatSignPadAdaptor.dll
```

### SignPadPicker.NicePosSignPadAdaptor plug-in library

- Vendor: NICE정보통신(서울전자통신)
- Client
  - 성남시의료원
  - 강원대병원

```
│  NICEPOSICV105.dll
└─ SignPadPicker.NicePosSignPadAdaptor.dll
```

### SignPadPicker.SmartroSignPadAdaptor plug-in library

- Vendor: 한우리IT
- Client
  - 충북대병원
  - 강원대병원
