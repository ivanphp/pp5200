# 介紹如何使用 ESC/POS 進行 LOGO 列印 (PP-5200F)
+ PP_BMP_v3000.rar 是載入NV-IMAGE 到 PP-5200F 工具壓縮檔
+ PP-5200F-NV載入.docx 操作說明

# ESC Print NV bit Image
**ASCII FS p n m**\
**Hex 1C 70 n m**

C# Code\
public static byte[] PrintNV = new byte[] { 0x1C, 0x70, 0x01, 0x00 };  
 + n specifies the NV bit image number.(start at 1)
 + m specifies the bit-image mode.(0 ≤ m ≤ 3, 48 ≤ m ≤ 51)

|      m         |            Mode              |
|----------------|------------------------------|
|0,48            |Normal Mode                   |
|1,49            |Double-wide Mode              |
|2,50            |Double-tall Mode              |
|3,51            |Quadruple Mode              |
