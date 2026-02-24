using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace PlayerChoice.DataSets;

public interface ISocialSitePost
{
	// If SocialPost_Type is NOT text, the SocialPost_Content is file path and SocialPost_Description is NULL
	public void Show_SocialPost(EnumStructs.E_PostType SocialPost_Type, string SocialPost_Content, string SocialPost_Description, string SocialPost_UserName, string SocialPost_PfP);
}

public class DataFunctions
{
	public delegate void D_SendSimulationInfo(EnumStructs.S_StatData[] StatAlterations);

	public static event D_SendSimulationInfo E_SendSimulationInfo;


	public delegate void D_SendSocialSitePost(EnumStructs.E_PostType SocialPost_Type, string SocialPost_Content, string SocialPost_Description, string SocialPost_UserName, string SocialPost_PfP);

	public static event D_SendSocialSitePost E_SendSocialSitePost;


	//-----------------------------------------


	public static SocialPost_JSON LoadJSONFile(string PAR_FileName)
	{
		StreamReader V_StrRead_SocialJSON	= new StreamReader(File.OpenRead(PAR_FileName));

		SocialPost_JSON V_SocialJSON		= JsonSerializer.Deserialize<SocialPost_JSON>(V_StrRead_SocialJSON.ReadToEnd());

		return V_SocialJSON;
	}

	private static void SpawnSocialPost(SocialPost_JSON PAR_SocialPost)
	{
		EnumStructs.E_PostType V_PostType	= EnumStructs.E_PostType.Text;

		if(PAR_SocialPost.Type				= true)
		{
			V_PostType						=
				PAR_SocialPost.Content.EndsWith(".mp4")	== true ?
					EnumStructs.E_PostType.Video
					:
					EnumStructs.E_PostType.Image;
		}

		E_SendSocialSitePost.Invoke(V_PostType, PAR_SocialPost.Content, PAR_SocialPost.Description, PAR_SocialPost.UserName, PAR_SocialPost.UserImage);
	}


	public static void StatAlteration(EnumStructs.S_StatData[] PAR_StatAlterations)
	{
		E_SendSimulationInfo.Invoke(PAR_StatAlterations);
	}
}