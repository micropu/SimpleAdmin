﻿using NewLife.Reflection;
using NewLife.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleSqlSugar;
using SimpleTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAdmin.Core.Utils;

/// <summary>
/// 种子数据工具类
/// </summary>
public class SeedDataUtil
{
    public static List<T> GetSeedData<T>(string jsonName)
    {

        var seedData = new List<T>();//种子数据结果
        var basePath = AppContext.BaseDirectory;//获取项目目录
        var json = basePath.CombinePath("SeedData", "Json", jsonName);//获取文件路径
        var dataString = FileHelper.ReadFile(json);//读取文件
        if (!string.IsNullOrEmpty(dataString))//如果有内容
        {

            //字段没有数据的替换成null
            dataString = dataString.Replace("\"\"", "null");
            //将json字符串转为实体，这里extjson可以正常转换为字符串
            var seedDataRecord1 = dataString.ToJsonEntity<SeedDataRecords<T>>();
            #region  针对导出的json字符串嵌套json字符串如 "DefaultDataScope": "{\"Level\":5,\"ScopeCategory\":\"SCOPE_ALL\",\"ScopeDefineOrgIdList\":[]}"
            //字符串是\"的替换成"
            dataString = dataString.Replace("\\\"", "\"");
            //字符串是\{替换成{
            dataString = dataString.Replace("\"{", "{");
            //字符串是}"的替换成}
            dataString = dataString.Replace("}\"", "}");
            //将json字符串转为实体,这里extjson会转为null，替换字符串把extjson值变为实体类型而实体类是string类型
            var seedDataRecord2 = dataString.ToJsonEntity<SeedDataRecords<T>>();
            #endregion
            //遍历seedDataRecord2
            for (int i = 0; i < seedDataRecord2.Records.Count; i++)
            {
                #region 处理ExtJosn
                //获取extjson属性
                var propertyExtJosn = typeof(T).GetProperty(nameof(PrimaryKeyEntity.ExtJson));
                if (propertyExtJosn != null)
                {
                    //获取extjson的值
                    var extJson = propertyExtJosn.GetValue(seedDataRecord2.Records[i])?.ToString();
                    // 如果extjson不为空并且包含NullableDictionary表示序列化失败了
                    if (!string.IsNullOrEmpty(extJson) && extJson.Contains("NullableDictionary"))
                    {
                        //设置extjson为seedDataRecord1对应的值
                        extJson = propertyExtJosn.GetValue(seedDataRecord1.Records[i])?.ToString();
                        //seedDataRecord2赋值seedDataRecord1的extjson
                        propertyExtJosn.SetValue(seedDataRecord2.Records[i], extJson);
                    }
                }

                #endregion
                #region 处理ConfigValue
                //获取extjson属性
                var propertyConfigValue = typeof(T).GetProperty(nameof(DevConfig.ConfigValue));
                if (propertyConfigValue != null)
                {
                    //获取extjson的值
                    var configValue = propertyConfigValue.GetValue(seedDataRecord2.Records[i])?.ToString();
                    // 如果extjson不为空并且包含NullableDictionary表示序列化失败了
                    if (!string.IsNullOrEmpty(configValue) && configValue.Contains("NullableDictionary"))
                    {
                        //设置extjson为seedDataRecord1对应的值
                        configValue = propertyConfigValue.GetValue(seedDataRecord1.Records[i])?.ToString();
                        //seedDataRecord2赋值seedDataRecord1的extjson
                        propertyConfigValue.SetValue(seedDataRecord2.Records[i], configValue);
                    }
                }
                #endregion
            }
            //种子数据赋值
            seedData = seedDataRecord2.Records;
        }

        return seedData;
    }
}

/// <summary>
/// 种子数据格式实体类,遵循Navicat导出json格式
/// </summary>
/// <typeparam name="T"></typeparam>

public class SeedDataRecords<T>
{
    /// <summary>
    /// 数据
    /// </summary>
    public List<T> Records { get; set; }
}
