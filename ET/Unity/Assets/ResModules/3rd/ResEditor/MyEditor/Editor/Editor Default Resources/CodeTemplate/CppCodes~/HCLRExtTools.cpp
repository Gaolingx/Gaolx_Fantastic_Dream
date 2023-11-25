#include "os/c-api/il2cpp-config-api-platforms.h"

#include "lzma/LzmaUtil.h"
extern "C"
{
    IL2CPP_EXPORT unsigned char* HCLRExtTools_Decompress(unsigned char* pSrc, int nLength, int* outLen)
    {
        return LzmaWrapper::LzmaDecompress(pSrc, nLength, outLen);
    }
}

#include "xxtea/xxtea.h"
extern "C"
{
    IL2CPP_EXPORT unsigned char* HCLRExtTools_XXTeaEncrypt(unsigned char* pSrc, unsigned int nLength, unsigned char *key, xxtea_long key_len, xxtea_long* outLen)
    {
        return XXTEAWrapper::xxtea_encrypt(pSrc, nLength, key, key_len, outLen);
    }

   IL2CPP_EXPORT unsigned char* HCLRExtTools_XXTeaDecrypt(unsigned char* pSrc, xxtea_long nLength, unsigned char *key, xxtea_long key_len, xxtea_long* outLen)
   {
      return XXTEAWrapper::xxtea_decrypt(pSrc, nLength, key, key_len, outLen);
   }
}
