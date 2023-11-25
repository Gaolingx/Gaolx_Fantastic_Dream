#pragma once

class LzmaWrapper
{
public:
    static unsigned char *LzmaDecompress(unsigned char *pSrc, int nLen, int *outLen);
};
