using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

using CaptureScreen;

class ConvertTo8Bit
{
    public static void SetPallete(uint[] cols)
    {
        cols[0] = 4278190080;
        cols[1] = 4278190165;
        cols[2] = 4278190250;
        cols[3] = 4278190335;
        cols[4] = 4278199296;
        cols[5] = 4278199381;
        cols[6] = 4278199466;
        cols[7] = 4278199551;
        cols[8] = 4278208768;
        cols[9] = 4278208853;
        cols[10] = 4278208938;
        cols[11] = 4278209023;
        cols[12] = 4278217984;
        cols[13] = 4278218069;
        cols[14] = 4278218154;
        cols[15] = 4278218239;
        cols[16] = 4278227456;
        cols[17] = 4278227541;
        cols[18] = 4278227626;
        cols[19] = 4278227711;
        cols[20] = 4278236672;
        cols[21] = 4278236757;
        cols[22] = 4278236842;
        cols[23] = 4278236927;
        cols[24] = 4278246144;
        cols[25] = 4278246229;
        cols[26] = 4278246314;
        cols[27] = 4278246399;
        cols[28] = 4278255360;
        cols[29] = 4278255445;
        cols[30] = 4278255530;
        cols[31] = 4278255615;
        cols[32] = 4280549376;
        cols[33] = 4280549461;
        cols[34] = 4280549546;
        cols[35] = 4280549631;
        cols[36] = 4280558592;
        cols[37] = 4280558677;
        cols[38] = 4280558762;
        cols[39] = 4280558847;
        cols[40] = 4280568064;
        cols[41] = 4280568149;
        cols[42] = 4280568234;
        cols[43] = 4280568319;
        cols[44] = 4280577280;
        cols[45] = 4280577365;
        cols[46] = 4280577450;
        cols[47] = 4280577535;
        cols[48] = 4280586752;
        cols[49] = 4280586837;
        cols[50] = 4280586922;
        cols[51] = 4280587007;
        cols[52] = 4280595968;
        cols[53] = 4280596053;
        cols[54] = 4280596138;
        cols[55] = 4280596223;
        cols[56] = 4280605440;
        cols[57] = 4280605525;
        cols[58] = 4280605610;
        cols[59] = 4280605695;
        cols[60] = 4280614656;
        cols[61] = 4280614741;
        cols[62] = 4280614826;
        cols[63] = 4280614911;
        cols[64] = 4282974208;
        cols[65] = 4282974293;
        cols[66] = 4282974378;
        cols[67] = 4282974463;
        cols[68] = 4282983424;
        cols[69] = 4282983509;
        cols[70] = 4282983594;
        cols[71] = 4282983679;
        cols[72] = 4282992896;
        cols[73] = 4282992981;
        cols[74] = 4282993066;
        cols[75] = 4282993151;
        cols[76] = 4283002112;
        cols[77] = 4283002197;
        cols[78] = 4283002282;
        cols[79] = 4283002367;
        cols[80] = 4283011584;
        cols[81] = 4283011669;
        cols[82] = 4283011754;
        cols[83] = 4283011839;
        cols[84] = 4283020800;
        cols[85] = 4283020885;
        cols[86] = 4283020970;
        cols[87] = 4283021055;
        cols[88] = 4283030272;
        cols[89] = 4283030357;
        cols[90] = 4283030442;
        cols[91] = 4283030527;
        cols[92] = 4283039488;
        cols[93] = 4283039573;
        cols[94] = 4283039658;
        cols[95] = 4283039743;
        cols[96] = 4285333504;
        cols[97] = 4285333589;
        cols[98] = 4285333674;
        cols[99] = 4285333759;
        cols[100] = 4285342720;
        cols[101] = 4285342805;
        cols[102] = 4285342890;
        cols[103] = 4285342975;
        cols[104] = 4285352192;
        cols[105] = 4285352277;
        cols[106] = 4285352362;
        cols[107] = 4285352447;
        cols[108] = 4285361408;
        cols[109] = 4285361493;
        cols[110] = 4285361578;
        cols[111] = 4285361663;
        cols[112] = 4285370880;
        cols[113] = 4285370965;
        cols[114] = 4285371050;
        cols[115] = 4285371135;
        cols[116] = 4285380096;
        cols[117] = 4285380181;
        cols[118] = 4285380266;
        cols[119] = 4285380351;
        cols[120] = 4285389568;
        cols[121] = 4285389653;
        cols[122] = 4285389738;
        cols[123] = 4285389823;
        cols[124] = 4285398784;
        cols[125] = 4285398869;
        cols[126] = 4285398954;
        cols[127] = 4285399039;
        cols[128] = 4287758336;
        cols[129] = 4287758421;
        cols[130] = 4287758506;
        cols[131] = 4287758591;
        cols[132] = 4287767552;
        cols[133] = 4287767637;
        cols[134] = 4287767722;
        cols[135] = 4287767807;
        cols[136] = 4287777024;
        cols[137] = 4287777109;
        cols[138] = 4287777194;
        cols[139] = 4287777279;
        cols[140] = 4287786240;
        cols[141] = 4287786325;
        cols[142] = 4287786410;
        cols[143] = 4287786495;
        cols[144] = 4287795712;
        cols[145] = 4287795797;
        cols[146] = 4287795882;
        cols[147] = 4287795967;
        cols[148] = 4287804928;
        cols[149] = 4287805013;
        cols[150] = 4287805098;
        cols[151] = 4287805183;
        cols[152] = 4287814400;
        cols[153] = 4287814485;
        cols[154] = 4287814570;
        cols[155] = 4287814655;
        cols[156] = 4287823616;
        cols[157] = 4287823701;
        cols[158] = 4287823786;
        cols[159] = 4287823871;
        cols[160] = 4290117632;
        cols[161] = 4290117717;
        cols[162] = 4290117802;
        cols[163] = 4290117887;
        cols[164] = 4290126848;
        cols[165] = 4290126933;
        cols[166] = 4290127018;
        cols[167] = 4290127103;
        cols[168] = 4290136320;
        cols[169] = 4290136405;
        cols[170] = 4290136490;
        cols[171] = 4290136575;
        cols[172] = 4290145536;
        cols[173] = 4290145621;
        cols[174] = 4290145706;
        cols[175] = 4290145791;
        cols[176] = 4290155008;
        cols[177] = 4290155093;
        cols[178] = 4290155178;
        cols[179] = 4290155263;
        cols[180] = 4290164224;
        cols[181] = 4290164309;
        cols[182] = 4290164394;
        cols[183] = 4290164479;
        cols[184] = 4290173696;
        cols[185] = 4290173781;
        cols[186] = 4290173866;
        cols[187] = 4290173951;
        cols[188] = 4290182912;
        cols[189] = 4290182997;
        cols[190] = 4290183082;
        cols[191] = 4290183167;
        cols[192] = 4292542464;
        cols[193] = 4292542549;
        cols[194] = 4292542634;
        cols[195] = 4292542719;
        cols[196] = 4292551680;
        cols[197] = 4292551765;
        cols[198] = 4292551850;
        cols[199] = 4292551935;
        cols[200] = 4292561152;
        cols[201] = 4292561237;
        cols[202] = 4292561322;
        cols[203] = 4292561407;
        cols[204] = 4292570368;
        cols[205] = 4292570453;
        cols[206] = 4292570538;
        cols[207] = 4292570623;
        cols[208] = 4292579840;
        cols[209] = 4292579925;
        cols[210] = 4292580010;
        cols[211] = 4292580095;
        cols[212] = 4292589056;
        cols[213] = 4292589141;
        cols[214] = 4292589226;
        cols[215] = 4292589311;
        cols[216] = 4292598528;
        cols[217] = 4292598613;
        cols[218] = 4292598698;
        cols[219] = 4292598783;
        cols[220] = 4292607744;
        cols[221] = 4292607829;
        cols[222] = 4292607914;
        cols[223] = 4292607999;
        cols[224] = 4294901760;
        cols[225] = 4294901845;
        cols[226] = 4294901930;
        cols[227] = 4294902015;
        cols[228] = 4294910976;
        cols[229] = 4294911061;
        cols[230] = 4294911146;
        cols[231] = 4294911231;
        cols[232] = 4294920448;
        cols[233] = 4294920533;
        cols[234] = 4294920618;
        cols[235] = 4294920703;
        cols[236] = 4294929664;
        cols[237] = 4294929749;
        cols[238] = 4294929834;
        cols[239] = 4294929919;
        cols[240] = 4294939136;
        cols[241] = 4294939221;
        cols[242] = 4294939306;
        cols[243] = 4294939391;
        cols[244] = 4294948352;
        cols[245] = 4294948437;
        cols[246] = 4294948522;
        cols[247] = 4294948607;
        cols[248] = 4294957824;
        cols[249] = 4294957909;
        cols[250] = 4294957994;
        cols[251] = 4294958079;
        cols[252] = 4294967040;
        cols[253] = 4294967125;
        cols[254] = 4294967210;
        cols[255] = 4294967295;

    }

    public static PlatformInvokeGDI32.BITMAPINFO bmi;
    public static bool first = true;
    public static IntPtr hbm0 = IntPtr.Zero;

    public static void Stop()
    {
        if (hbm0 != IntPtr.Zero)
        {
            PlatformInvokeGDI32.DeleteObject(hbm0);
            hbm0 = IntPtr.Zero;
        }
        first = true;
    }

    // Быстрое конвертирование
    // http://www.wischik.com/lu/programmer/1bpp.html
    public static int CopyToBpp(IntPtr hbm, int W, int H, int bpp, ref Bitmap active_screen)
    {
        if (bpp != 1 && bpp != 8) throw new System.ArgumentException("1 or 8", "bpp");

        // Plan: built into Windows GDI is the ability to convert
        // bitmaps from one format to another. Most of the time, this
        // job is actually done by the graphics hardware accelerator card
        // and so is extremely fast. The rest of the time, the job is done by
        // very fast native code.
        // We will call into this GDI functionality from C#. Our plan:
        // (1) Convert our Bitmap into a GDI hbitmap (ie. copy unmanaged->managed)
        // (2) Create a GDI monochrome hbitmap
        // (3) Use GDI "BitBlt" function to copy from hbitmap into monochrome (as above)
        // (4) Convert the monochrone hbitmap into a Bitmap (ie. copy unmanaged->managed)

        int w = W, h = H;
        if (first)
        {
            first = false;
            //
            // Step (2): create the monochrome bitmap.
            // "BITMAPINFO" is an interop-struct which we define below.
            // In GDI terms, it's a BITMAPHEADERINFO followed by an array of two RGBQUADs
            bmi = new PlatformInvokeGDI32.BITMAPINFO();
            bmi.biSize = 40;  // the size of the BITMAPHEADERINFO struct
            bmi.biWidth = w;
            bmi.biHeight = -h;
            bmi.biPlanes = 1; // "planes" are confusing. We always use just 1. Read MSDN for more info.
            bmi.biBitCount = (short)bpp; // ie. 1bpp or 8bpp
            bmi.biCompression = PlatformInvokeGDI32.BI_RGB; // ie. the pixels in our RGBQUAD table are stored as RGBs, not palette indexes
            bmi.biSizeImage = (uint)(((w + 7) & 0xFFFFFFF8) * h / 8);
            bmi.biXPelsPerMeter = 1000000; // not really important
            bmi.biYPelsPerMeter = 1000000; // not really important
            // Now for the colour table.
            uint ncols = (uint)1 << bpp; // 2 colours for 1bpp; 256 colours for 8bpp
            bmi.biClrUsed = ncols;
            bmi.biClrImportant = ncols;
            bmi.cols = new uint[256]; // The structure always has fixed size 256, even if we end up using fewer colours
            if (bpp == 1) { bmi.cols[0] = MAKERGB(0, 0, 0); bmi.cols[1] = MAKERGB(255, 255, 255); }
            else
            {
                // ставим подготовленную палитру
                bmi.biClrUsed = 256; bmi.biClrImportant = 256;
                SetPallete(bmi.cols);
            }
            // For 8bpp we've created an palette with just greyscale colours.
            // You can set up any palette you want here. Here are some possibilities:
            // greyscale: for (int i=0; i<256; i++) bmi.cols[i]=MAKERGB(i,i,i);
            // rainbow: bmi.biClrUsed=216; bmi.biClrImportant=216; int[] colv=new int[6]{0,51,102,153,204,255};
            //          for (int i=0; i<216; i++) bmi.cols[i]=MAKERGB(colv[i/36],colv[(i/6)%6],colv[i%6]);
            // optimal: a difficult topic: http://en.wikipedia.org/wiki/Color_quantization
            // 
            // Now create the indexed bitmap "hbm0"            
        }

        if (hbm0 == IntPtr.Zero)
        {
            IntPtr bits0; // not used for our purposes. It returns a pointer to the raw bits that make up the bitmap.
            hbm0 = PlatformInvokeGDI32.CreateDIBSection(IntPtr.Zero, ref bmi, PlatformInvokeGDI32.DIB_RGB_COLORS, out bits0, IntPtr.Zero, 0);
        }

        //
        IntPtr hdc = IntPtr.Zero;
        IntPtr hdc0 = IntPtr.Zero;
        IntPtr sdc = IntPtr.Zero;

        // Step (3): use GDI's BitBlt function to copy from original hbitmap into monocrhome bitmap
        // GDI programming is kind of confusing... nb. The GDI equivalent of "Graphics" is called a "DC".
        sdc = PlatformInvokeUSER32.GetDC(IntPtr.Zero);       // First we obtain the DC for the screen
        // Next, create a DC for the original hbitmap
        hdc = PlatformInvokeGDI32.CreateCompatibleDC(sdc); PlatformInvokeGDI32.SelectObject(hdc, hbm);
        // and create a DC for the monochrome hbitmap
        hdc0 = PlatformInvokeGDI32.CreateCompatibleDC(sdc); PlatformInvokeGDI32.SelectObject(hdc0, hbm0);

        // Now we can do the BitBlt:
        PlatformInvokeGDI32.BitBlt(hdc0, 0, 0, w, h, hdc, 0, 0, 0x00CC0020 | 0x40000000);

        active_screen = System.Drawing.Bitmap.FromHbitmap(hbm0);

        // Finally some cleanup.
        PlatformInvokeGDI32.DeleteDC(hdc);
        PlatformInvokeGDI32.DeleteDC(hdc0);
        if (sdc != IntPtr.Zero)
        {
            PlatformInvokeUSER32.ReleaseDC(IntPtr.Zero, sdc);
        }

        return 0;
    }

       static public uint MAKERGB(int r, int g, int b)
    {
        return ((uint)(b & 255)) | ((uint)((g & 255) << 16)) | ((uint)((r & 255) << 8));
    }
}
