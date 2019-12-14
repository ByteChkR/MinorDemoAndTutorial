

__kernel void kernel_red(__global uchar* imageData, float strength, uchar channelCount)
{
	int idx = get_global_id(0);
	int channel = (int)fmod((float)idx, (float)channelCount);
	if(channel != 2)//BG[R]A
	{
		return;
	}
	imageData[idx] = (uchar)(255*strength);
}