using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CycleHelper
{
  public class Function
  {

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
    {
      var intentRequest = input.Request as IntentRequest;
      return ResponseBuilder.Tell(new SsmlOutputSpeech(GetMeteo(intentRequest.Intent.Slots["cityName"].Value)));
    }

    String GetMeteo(String cityName)
    {
      //
      using (var httpClient = new HttpClient())
      {

        var result = httpClient.GetAsync($"https://geo.api.gouv.fr/communes?nom={cityName}&fields=nom,code,codesPostaux,codeDepartement,codeRegion,population&format=json&geometry=centre").Result;
        var city = JsonConvert.DeserializeObject<City[]>(result.Content.ReadAsStringAsync().Result).FirstOrDefault();


        result = httpClient.GetAsync($"http://www.meteofrance.com/mf3-rpc-portlet/rest/pluie/{city.code}0").Result;
        var meteo = JsonConvert.DeserializeObject<Meteo>(result.Content.ReadAsStringAsync().Result);

        return meteo.niveauPluieText.FirstOrDefault();
      }
    }

  }

  public class City
  {
    public String nom { get; set; }
    public String code { get; set; }
    public List<String> codesPostaux { get; set; }
    public String codeDepartement { get; set; }
    public String codeRegion { get; set; }
    public Int32 population { get; set; }
    public Int32 _score { get; set; }
  }
  public class DataCadran
  {
    public String niveauPluieText { get; set; }
    public Int32 niveauPluie { get; set; }
    public String color { get; set; }
  }

  public class Meteo
  {
    public String idLieu { get; set; }
    public String echeance { get; set; }
    public String lastUpdate { get; set; }
    public Boolean isAvailable { get; set; }
    public Boolean hasData { get; set; }
    public List<String> niveauPluieText { get; set; }
    public List<DataCadran> dataCadran { get; set; }
  }
}
