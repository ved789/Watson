using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using ConfirmWatson.ConnectorService;
using ConfirmWatsonEntity;
using MapInfo.Engine;
using MapInfo.Geometry;

namespace ConfirmWatson
{
  public class EnquiryRaiser
  {
    public const string MapInfoBritishNationalGrid = "mapinfo:coordsys 8,79,7,-2,49,0.9996012717,400000,-100000";
    public const int SearchDistance = 10;
    public static string Create(string docLoc) //, string enqNum = "", string enqX = "", string enqY = "", string centralAssetId = "")
    {
      string enqNum = DateTime.Now.ToShortDateString().Replace("2018", "").Replace("/","").Trim() + DateTime.Now.ToLongTimeString().Replace(":", "").Replace("AM","").Replace("PM", "").Trim();
      double x = Convert.ToDouble("278437.99");
      double y = Convert.ToDouble("187567.50");
      string centralAssetId = AssetSearch(x, y, SearchDistance); //"00060836";
      //EnquiryNumber, LoggedTime, EnquiryX, EnquiryY, CentralAssetId, DocumentLocation
      string xml = File.ReadAllText("NewEnquiryCrmAgentReq.xml");
      xml = string.Format(xml, enqNum, DateTime.Now, x, y, centralAssetId, docLoc);
      XmlDocument document = new XmlDocument();
      document.LoadXml(xml); 
      
      ConfirmConnectorSoap service = new ConfirmConnectorSoapClient();
      ProcessOperationsResponse resonse = service.ProcessOperations(new ProcessOperationsRequest(document.DocumentElement.ParentNode));
      ConfirmWatsonEntity.OperationResponse createEnquiryResponse = Deserialize<ConfirmWatsonEntity.OperationResponse>(resonse.ProcessOperationsResult.InnerXml);
      Enquiry enquiry = createEnquiryResponse.NewEnquiryResponse.Enquiry;
      return enquiry.EnquiryNumber;
    }

    public static string AssetSearch(double x, double y, int dist)
    {
      //string boundX2, string boundY2
      CoordSys coordSys = Session.Current.CoordSysFactory.CreateCoordSys(MapInfoBritishNationalGrid); //BNG
      Distance distance = new Distance(dist, DistanceUnit.Meter);
      double searchDistance = distance.Value;
      CoordinateTransform transform = new CoordinateTransform(coordSys, coordSys);
      DPoint dPoint = new DPoint(x, y);
      DPoint convertedPoint = transform.CoordSys1ToCoordSys2(dPoint);
      var pointX1Y1 = new DPoint(convertedPoint.x + searchDistance, convertedPoint.y + searchDistance);
      var pointX2Y2 = new DPoint(convertedPoint.x - searchDistance, convertedPoint.y - searchDistance);
      Rectangle searchRect = new Rectangle(coordSys, new DRect(pointX1Y1, pointX2Y2));

      string xml = File.ReadAllText("AssetSearch.xml");
      xml = string.Format(xml, searchRect.Bounds.x1, searchRect.Bounds.y1, searchRect.Bounds.x2, searchRect.Bounds.y2);
      XmlDocument document = new XmlDocument();
      document.LoadXml(xml);

      ConfirmConnectorSoap service = new ConfirmConnectorSoapClient();
      ProcessOperationsResponse resonse = service.ProcessOperations(new ProcessOperationsRequest(document.DocumentElement.ParentNode));
      OperationResponse assetSearchResponse = Deserialize<OperationResponse>(resonse.ProcessOperationsResult.InnerXml);
      var centralAssetId = assetSearchResponse.AssetSearchResponse.Asset.FirstOrDefault()?.CentralAssetId;
      return centralAssetId;
    }

    public static T Deserialize<T>(string input) where T : class
    {
      XmlSerializer ser = new XmlSerializer(typeof(T));

      using (StringReader sr = new StringReader(input))
      {
        return (T)ser.Deserialize(sr);
      }
    }
  }

  [XmlRoot(ElementName = "Asset")]
  public class Asset
  {
    [XmlElement(ElementName = "CentralAssetId")]
    public string CentralAssetId { get; set; }
    [XmlElement(ElementName = "FeatureId")]
    public string FeatureId { get; set; }
    [XmlElement(ElementName = "FeatureLocation")]
    public string FeatureLocation { get; set; }
    [XmlElement(ElementName = "FeatureTypeName")]
    public string FeatureTypeName { get; set; }
    [XmlElement(ElementName = "AddressReference")]
    public string AddressReference { get; set; }
    [XmlElement(ElementName = "FeatureX")]
    public string FeatureX { get; set; }
    [XmlElement(ElementName = "FeatureY")]
    public string FeatureY { get; set; }
    [XmlElement(ElementName = "WKT")]
    public string WKT { get; set; }
  }

  [XmlRoot(ElementName = "AssetSearchResponse")]
  public class AssetSearchResponse
  {
    [XmlElement(ElementName = "Asset")]
    public List<Asset> Asset { get; set; }
  }

  [XmlRoot(ElementName = "OperationResponse")]
  public class OperationResponse
  {
    [XmlElement(ElementName = "AssetSearchResponse")]
    public AssetSearchResponse AssetSearchResponse { get; set; }
  }
}

namespace ConfirmWatsonEntity
{
  [XmlRoot(ElementName = "Enquiry")]
  public class Enquiry
  {
    [XmlElement(ElementName = "EnquiryNumber")]
    public string EnquiryNumber { get; set; }
    [XmlElement(ElementName = "ServiceCode")]
    public string ServiceCode { get; set; }
    [XmlElement(ElementName = "ServiceName")]
    public string ServiceName { get; set; }
    [XmlElement(ElementName = "SubjectCode")]
    public string SubjectCode { get; set; }
    [XmlElement(ElementName = "SubjectName")]
    public string SubjectName { get; set; }
    [XmlElement(ElementName = "EnquiryDescription")]
    public string EnquiryDescription { get; set; }
    [XmlElement(ElementName = "EnquiryLocation")]
    public string EnquiryLocation { get; set; }
    [XmlElement(ElementName = "EnquiryLogNumber")]
    public string EnquiryLogNumber { get; set; }
    [XmlElement(ElementName = "EnquiryStatusCode")]
    public string EnquiryStatusCode { get; set; }
    [XmlElement(ElementName = "EnquiryStatusName")]
    public string EnquiryStatusName { get; set; }
    [XmlElement(ElementName = "AssignedOfficerCode")]
    public string AssignedOfficerCode { get; set; }
    [XmlElement(ElementName = "AssignedOfficerName")]
    public string AssignedOfficerName { get; set; }
    [XmlElement(ElementName = "LoggedTime")]
    public string LoggedTime { get; set; }
    [XmlElement(ElementName = "LogEffectiveTime")]
    public string LogEffectiveTime { get; set; }
    [XmlElement(ElementName = "SiteCode")]
    public string SiteCode { get; set; }
    [XmlElement(ElementName = "SiteName")]
    public string SiteName { get; set; }
    [XmlElement(ElementName = "SiteLocalityName")]
    public string SiteLocalityName { get; set; }
    [XmlElement(ElementName = "SiteTownName")]
    public string SiteTownName { get; set; }
    [XmlElement(ElementName = "SiteCountyName")]
    public string SiteCountyName { get; set; }
    [XmlElement(ElementName = "SiteClassCode")]
    public string SiteClassCode { get; set; }
    [XmlElement(ElementName = "SiteClassName")]
    public string SiteClassName { get; set; }
    [XmlElement(ElementName = "SiteLogNotes")]
    public string SiteLogNotes { get; set; }
    [XmlElement(ElementName = "StatusFollowUpTime")]
    public string StatusFollowUpTime { get; set; }
    [XmlElement(ElementName = "EnquiryLogTime")]
    public string EnquiryLogTime { get; set; }
    [XmlElement(ElementName = "EnquiryReference")]
    public string EnquiryReference { get; set; }
    [XmlElement(ElementName = "EnquiryClassCode")]
    public string EnquiryClassCode { get; set; }
    [XmlElement(ElementName = "EnquiryClassName")]
    public string EnquiryClassName { get; set; }
    [XmlElement(ElementName = "ContactName")]
    public string ContactName { get; set; }
    [XmlElement(ElementName = "ContactPhone")]
    public string ContactPhone { get; set; }
    [XmlElement(ElementName = "ContactAltPhone")]
    public string ContactAltPhone { get; set; }
    [XmlElement(ElementName = "ContactFax")]
    public string ContactFax { get; set; }
    [XmlElement(ElementName = "ContactEmail")]
    public string ContactEmail { get; set; }
    [XmlElement(ElementName = "EnquiryX")]
    public string EnquiryX { get; set; }
    [XmlElement(ElementName = "EnquiryY")]
    public string EnquiryY { get; set; }
    [XmlElement(ElementName = "LoggedByUserName")]
    public string LoggedByUserName { get; set; }
    [XmlElement(ElementName = "LoggedByUserId")]
    public string LoggedByUserId { get; set; }
    [XmlElement(ElementName = "JobNumber")]
    public string JobNumber { get; set; }
    [XmlElement(ElementName = "JobStartDate")]
    public string JobStartDate { get; set; }
    [XmlElement(ElementName = "JobEndDate")]
    public string JobEndDate { get; set; }
  }

  [XmlRoot(ElementName = "NewEnquiryResponse")]
  public class NewEnquiryResponse
  {
    [XmlElement(ElementName = "Enquiry")]
    public Enquiry Enquiry { get; set; }
  }

  [XmlRoot(ElementName = "OperationResponse")]
  public class OperationResponse
  {
    [XmlElement(ElementName = "NewEnquiryResponse")]
    public NewEnquiryResponse NewEnquiryResponse { get; set; }
  }

}
