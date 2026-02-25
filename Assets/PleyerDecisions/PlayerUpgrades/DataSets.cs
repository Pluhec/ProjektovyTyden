namespace PlayerChoice.DataSets
{
	public class SocialPost_JSON
	{
		public bool Type { get; set; }
		public EnumStructs.E_CampaignTopic Topic { get; set; }
		public EnumStructs.E_GameStage Agression { get; set; }
		public string Content { get; set; }
		public string Description { get; set; }
		public string UserImage { get; set; }
		public string UserName { get; set; }
	}
}