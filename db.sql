USE [h5.pay]
GO

/****** Object:  Table [dbo].[orderinfo]    Script Date: 06/07/2018 16:42:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[orderinfo](
	[orderno] [varchar](96) NOT NULL,
	[project] [varchar](20) NULL,
	[status] [varchar](50) NULL,
	[date] [datetime] NULL,
	[created] [datetime] NULL,
	[amount] [int] NULL,
	[paytype] [varchar](20) NULL,
	[paych] [varchar](20) NULL,
	[callbackdata] [varchar](50) NULL,
	[callbackdate] [datetime] NULL,
 CONSTRAINT [PK_orderinfo] PRIMARY KEY CLUSTERED 
(
	[orderno] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


