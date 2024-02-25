/*
    Copyright 2005,2006 Luigi Auriemma

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA

    http://www.gnu.org/licenses/gpl.txt
*/

#include <stdio.h>
#include <stdlib.h>
#include <sys/stat.h>
#include <ctype.h>
#include "mywav.h"
#include "uXboxAdpcmDecoder.h"

#ifndef NOPLAYSEEK      // use -DNOPLAYSEEK to disable the feature
    #define PLAYSEEK    // disable if the seeking feature gives problems!!!
#endif

#ifdef WIN32
    #include <conio.h>
    #include "ao.h"

    typedef unsigned char   u_char;
    typedef unsigned short  u_short;
    typedef unsigned int    u_int;

    #define KEYRIGHT    0x4d
    #define KEYLEFT     0x4b
    #define KEYUP       0x48
    #define KEYDOWN     0x50
    #define KEYPGUP     0x49
    #define KEYPGDOWN   0x51
#else
    #include <ao/ao.h>
    #ifdef PLAYSEEK
        #include "kbhit.h"
    #endif

    #define stricmp strcasecmp
    #define getch   readch

    #define KEYRIGHT    0x43
    #define KEYLEFT     0x44
    #define KEYUP       0x41
    #define KEYDOWN     0x42
    #define KEYPGUP     0x35
    #define KEYPGDOWN   0x36
#endif



#define VER         "0.2.3a"
#define XBOXTAG     0x69
#define FREQ        44100
#define CHANS       2



void showbar(int s, int st);
void show_ao_list(void);
int get_filesize(FILE *fd);
int copyfile(FILE *in, FILE *out, int size);
int get_num(char *str);
void ao_error(char *device);
void std_err(void);



int main(int argc, char *argv[]) {
	ao_device           *device;
	ao_sample_format    format;
    int                 ao_driver;

    mywav_fmtchunk  fmt;
    FILE    *fd,
            *fdo    = NULL;
    u_int   i,
            startfileoff;
    int     len,
            plen,
            refresh,
            glen,
            offset  = -1,
            size    = -1,
            wavsize,
            chans   = 0,
            freq    = 0,
            addhead = 0,
            morelen = 0,
            raw     = 0,
            play    = 0,
            basesz,
            insz,
            outsz;
    u_char  *infile,
            *outfile,
            *more   = NULL,
            *in,
            *out,
            *driver = NULL;


    setbuf(stdin,  NULL);
    setbuf(stdout, NULL);
    setbuf(stderr, NULL);

    fputs("\n"
        "Xbox ADPCM decoder and player "VER"\n"
        "by Luigi Auriemma\n"
        "e-mail: aluigi@autistici.org\n"
        "web:    aluigi.org\n"
        "\n", stderr);

    if(argc < 3) {
        fprintf(stderr,
            "\n"
            "Usage: %s [options] <input> <output.wav/PLAY>\n"
            "\n"
            "Options:\n"
            "-o OFF    offset where audio data starts\n"
            "-s SIZE   size of the audio data to read\n"
            "-f FREQ   force the frequency (default for raw files is %d)\n"
            "-c CHANS  force the number of channels (default for raw files is %d)\n"
            "-a        copy the input file to the output and add a wave header\n"
            "          useful if you have the xbox adpcm codec installed\n"
            "-r        raw output, data without wave header (useful for pipe)\n"
            "          if used together -r and -a act just like a data dumper\n"
            "-d DRV    select a specific driver for playing the file, DRV can be the short\n"
            "          name or the device number. Use ? for the list of devices\n"
            "\n"
            "Use the ouptut filename PLAY or \"\" for playing the input file (no writing)\n"
            "Use - as input or output file for using stdin and stdout\n"
            "Use 0x before any number for specyfing its hexadecimal value\n"
            "\n",
            argv[0], FREQ, CHANS);
        exit(1);
    }

    argc -= 2;
    for(i = 1; i < argc; i++) {
        if(argv[i][0] != '-') continue;
        switch(argv[i][1]) {
            case 'o': offset = get_num(argv[++i]);  break;
            case 's': size   = get_num(argv[++i]);  break;
            case 'f': freq   = get_num(argv[++i]);  break;
            case 'c': chans  = get_num(argv[++i]);  break;
            case 'a': {
                more    = "\x02\x00" "\x40\x00";
                morelen = 4;
                addhead = 1;
                } break;
            case 'r': raw    = 1;                   break;
            case 'd': {
                driver = argv[++i];
                if(!strcmp(driver, "?") || !stricmp(driver, "list") || !stricmp(driver, "help")) {
                    show_ao_list();
                    return(0);
                }
                } break;
            default: {
                fprintf(stderr, "\nError: wrong command-line argument (%s)\n\n", argv[i]);
                exit(1);
                } break;
        }
    }

    infile  = argv[argc];
    outfile = argv[argc + 1];

    if(!strcmp(infile, "-")) {
        fd = stdin;
    } else {
        fprintf(stderr, "- input file:    %s\n", infile);
        fd = fopen(infile, "rb");
        if(!fd) std_err();
    }

    if(offset >= 0) {
        if(fseek(fd, offset, SEEK_SET) < 0) std_err();
    }

    wavsize = mywav_data(fd, &fmt);
    if(wavsize < 0) {
        fprintf(stderr, "- input raw data\n");
        if(offset < 0) {
            fseek(fd, 0, SEEK_SET);
        } else {
            fseek(fd, offset, SEEK_SET);
        }
        if(!chans)   chans = CHANS;
        if(!freq)    freq  = FREQ;
        if(size < 0) size  = get_filesize(fd);

    } else {
        fprintf(stderr, "- input wave data\n");
        if(fmt.wFormatTag != XBOXTAG) {
            fprintf(stderr, "- Alert: input file doesn't use the XBOX codec, I try to continue\n");
        }
        fprintf(stderr,
            "  channels:      %hu\n"
            "  samples/sec:   %u\n"
            "  avg/bytes/sec: %u\n"
            "  block align:   %hu\n"
            "  bits:          %hu\n",
            fmt.wChannels,
            fmt.dwSamplesPerSec,
            fmt.dwAvgBytesPerSec,
            fmt.wBlockAlign,
            fmt.wBitsPerSample);
        if(!chans)   chans = fmt.wChannels;
        if(!freq)    freq  = fmt.dwSamplesPerSec;
        if(size < 0) size  = wavsize;
    }

    if(!strcmp(outfile, "-")) {
        fdo = stdout;
    } else if(!stricmp(outfile, "PLAY") || !outfile[0]) {
        play    = 1;
        addhead = 0;
        if(raw) {
            printf("\nError: you cannot play the raw input data on your device\n\n");
            exit(1);
        }
    } else {
        fprintf(stderr, "- output file:   %s\n", outfile);
        fdo = fopen(outfile, "rb");
        if(fdo) {
            fclose(fdo);
            printf("- the output file already exists, do you want to overwrite it? (y/N)\n  ");
            if(tolower(fgetc(stdin)) != 'y') {
                printf("- quit\n");
                return(0);
            }
        }
        fdo = fopen(outfile, "wb");
        if(!fdo) std_err();
    }

    if(addhead) {
        glen = size;
    } else {
        glen = TXboxAdpcmDecoder_guess_output_size(size);
    }

    fmt.wChannels                = chans;
    if(!raw) {
        fmt.dwSamplesPerSec      = freq;
        if(addhead) {
            fmt.wFormatTag       = XBOXTAG;
            fmt.wBitsPerSample   = 4;
            fmt.wBlockAlign      = fmt.wBitsPerSample * fmt.wChannels * 9;  // 9???
            fmt.dwAvgBytesPerSec = 49612;                                   // 49612???
        } else {
            fmt.wFormatTag       = 1;
            fmt.wBitsPerSample   = 16;
            fmt.wBlockAlign      = (fmt.wBitsPerSample / 8) * fmt.wChannels;
            fmt.dwAvgBytesPerSec = fmt.dwSamplesPerSec * fmt.wBlockAlign;
        }
        if(fdo) mywav_writehead(fdo, &fmt, glen, more, morelen);
    }

    if(play) {
        insz   = XBOX_ADPCM_SRCSIZE * fmt.wChannels;
        basesz = insz;
        outsz  = XBOX_ADPCM_DSTSIZE * fmt.wChannels;

        len    = 2048 / insz;   // this is the size of the block to read
        if(len > 1) {           // I use it for better performances
            insz  *= len;
            outsz *= len;
        }

        in  = malloc(insz);
        if(!in) std_err();
        out = malloc(outsz);
        if(!out) std_err();

        fprintf(stderr, "- initialize audio device\n");
        ao_initialize();
        if(driver) {
            ao_driver      = ao_driver_id(driver);
            if(ao_driver < 0) ao_driver = atoi(driver);
        } else {
            ao_driver      = ao_default_driver_id();
        }
        if(ao_driver < 0) ao_error(driver);

        format.bits        = fmt.wBitsPerSample;
        format.channels    = fmt.wChannels;
        format.rate        = fmt.dwSamplesPerSec;
        format.byte_format = AO_FMT_LITTLE;
        device = ao_open_live(ao_driver, &format, NULL);
        if(!device) ao_error("");

        fprintf(stderr, "- start playing:\n");
#ifdef PLAYSEEK
        fprintf(stderr, "  (use arrow keys and PGU and PGD for seeking and space for pause)\n");
#endif

        startfileoff = ftell(fd);
        plen = refresh = 0;

#ifdef PLAYSEEK
    #ifndef WIN32
        if(fd != stdin) init_keyboard();
    #endif
#endif

        while((len = fread(in, 1, insz, fd))) {
            len = TXboxAdpcmDecoder_Decode_Memory((void *)in, len, (void *)out, fmt.wChannels);
            if(!ao_play(device, out, len)) break;

            plen += len;
            if(plen >= refresh) {
                showbar(plen, glen);

                if(fd == stdin) goto skipseek;
#ifdef PLAYSEEK
                if(kbhit()) {
                    switch(getch()) {
                        case KEYRIGHT:  len =  (basesz * 3000);     break;
                        case KEYLEFT:   len = -(basesz * 3000);     break;
                        case KEYUP:     len =  (basesz * 15000);    break;
                        case KEYDOWN:   len = -(basesz * 15000);    break;
                        case KEYPGUP:   len =  (basesz * 75000);    break;
                        case KEYPGDOWN: len = -(basesz * 75000);    break;
                        case 0x03:
                        case 0x04:
                        case 'q':
                        case 'x':
                        case '\r':
                        case '\n':     goto stop_play;              break;
                        case ' ': {
                            fprintf(stderr, "  === PAUSE ===  \r");
                            fgetc(stdin);
                            len = 0;
                            } break;
                        default: {
                            len = 0;
                            break;
                        }
                    }
                    if(len) {
                        if(fseek(fd, len, SEEK_CUR) < 0) {
                            if((ftell(fd) + len) < 0) {
                                fseek(fd, startfileoff, SEEK_SET);
                                plen = 0;
                            } else {
                                goto stop_play;
                            }
                        } else {
                            plen += (len / XBOX_ADPCM_SRCSIZE) * XBOX_ADPCM_DSTSIZE;
                        }
                    }
                }
#endif

skipseek:
                refresh = plen + 8192;  // about half second
            }
        }

stop_play:
#ifdef PLAYSEEK
    #ifndef WIN32
        if(fd != stdin) close_keyboard();
    #endif
#endif

        showbar(plen, glen);
        fputc('\n', stderr);

        ao_close(device);
        ao_shutdown();
        fclose(fd);
        free(in);
        free(out);
        return(0);
    }

    if(addhead) {
        fprintf(stderr, "- start data copying:\n");
        len = copyfile(fd, fdo, size);
    } else {
        fprintf(stderr, "- start decoding:\n");
        len = TXboxAdpcmDecoder_Decode(fd, fdo, -1, size, fmt.wChannels);
    }

    if(glen != len) {
        fprintf(stderr, "- Alert: the output file size (%u) is different than how much it should be (%u)\n",
            len, glen);
        if(!raw) {
            fprintf(stderr, "- rewrite the correct wave header\n");
            fflush(fdo);
            fseek(fdo, 0, SEEK_SET);
            mywav_writehead(fdo, &fmt, len, more, morelen);
        }
    }

    fclose(fd);
    fclose(fdo);
    fprintf(stderr, "- decoded %u bytes\n", len);
    return(0);
}



void showbar(int s, int st) {
    int     perc;
    static  u_char  bar[80];
    u_char  *p,
            *l1,
            *l2;

    if(s > st) s = st;

    p = bar;
    p += sprintf(p, "  %3d%%  ", s / (st / 100));

    perc = s / (st / 65);

    l1 = p + perc;
    l2 = p + 65;
    while(p < l1) *p++ = '#';
    while(p < l2) *p++ = '.';
    *p++ = '\r';

    fwrite(bar, p - bar, 1, stderr);
}



void show_ao_list(void) {
    ao_info **list;
    int     i,
            num;

    ao_initialize();
    list = ao_driver_info_list(&num);
    fprintf(stderr, "- Devices:\n");
    for(i = 0; i < num; i++) {
        if(list[i]->type != AO_TYPE_LIVE) continue;
        fprintf(stderr, "  %d) %10s   %s\n", i, list[i]->short_name, list[i]->name);
    }
    ao_shutdown();
}



int get_filesize(FILE *fd) {
    struct  stat    xstat;

    fstat(fileno(fd), &xstat);
    return(xstat.st_size - ftell(fd));
}



int copyfile(FILE *in, FILE *out, int size) {
    int     len,
            fixed,
            tot = 0;
    u_char  buff[4096];

    for(fixed = sizeof(buff); size; size -= fixed) {
        if(size < fixed) fixed = size;
        len = fread(buff, 1, fixed, in);
        if(!len) break;
        fwrite(buff, len, 1, out);
        tot += len;
    }

    return(tot);
}



int get_num(char *str) {
    int     offset;

    if(!strncmp(str, "0x", 2) || !strncmp(str, "0X", 2)) {
        sscanf(str + 2, "%x", &offset);
    } else {
        sscanf(str, "%u", &offset);
    }
    return(offset);
}



void ao_error(char *device) {
    fprintf(stderr, "\nError: impossible to open the audio device %s\n", device);
    exit(1);
}



void std_err(void) {
    perror("\nError");
    exit(1);
}


