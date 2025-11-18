using System;

namespace LabTestPlatform.Data.Entities
{
    /// <summary>
    /// 平台实体类 - 对应数据库表 tb_platform
    /// </summary>
    public class PlatformEntity
    {
        /// <summary>
        /// 平台ID (主键)
        /// </summary>
        public int PlatformId { get; set; }

        /// <summary>
        /// 系统ID (外键)
        /// </summary>
        public int SystemId { get; set; }

        /// <summary>
        /// 平台编码 (唯一)
        /// </summary>
        public string PlatformCode { get; set; } = string.Empty;

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>
        /// 平台类型 (TESTING/PRODUCTION/DEVELOPMENT/QUALITY)
        /// </summary>
        public string? PlatformType { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// 平台描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 安装日期
        /// </summary>
        public DateTime? InstallDate { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 是否启用 (1:启用, 0:禁用)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 创建人
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
