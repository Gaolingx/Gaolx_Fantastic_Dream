#include "LzmaUtil.h"
#include "Precomp.h"
#include "CpuArch.h"
#include "Alloc.h"
#include "7zVersion.h"
#include "LzFind.h"
#include "LzmaDec.h"

unsigned char* LzmaWrapper::LzmaDecompress(unsigned char* pSrc, int nLen, int *outLen)
{
    *outLen = 0;
    if (!pSrc || nLen <= 0)
    {
        return NULL;
    }

    LzFindPrepare();

    int readPos = 13; //skip zip header
    int writePos = 0;

    UInt64 unpackSize = 0;
    for (int i = 0; i < 8; ++i)
    {
        unpackSize += (UInt64)pSrc[LZMA_PROPS_SIZE + i] << (i * 8);
    }

    CLzmaDec state{};
    state.dic = NULL;
    state.probs = NULL;

    if (LzmaDec_Allocate(&state, pSrc, LZMA_PROPS_SIZE, &g_Alloc) != 0)
    {
        return NULL;
    }
    LzmaDec_Init(&state);
    unsigned char* fileOut = new unsigned char[unpackSize] {0};

    SizeT inProcessed = nLen;
    SizeT outProcessed = unpackSize;
    ELzmaFinishMode finishMode = LZMA_FINISH_ANY;
    ELzmaStatus status;
    if (LzmaDec_DecodeToBuf(&state, fileOut, &outProcessed, pSrc + readPos, &inProcessed, finishMode, &status))
    {
        delete[] fileOut;
        return pSrc;
    }

    *outLen = (int)outProcessed;

    return fileOut;
}
