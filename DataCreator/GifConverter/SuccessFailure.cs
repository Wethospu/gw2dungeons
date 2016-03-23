
namespace GifConverter
{
  class SuccessFailure
  {
    public int Success = 0;
    public int Failure = 0;
    public int Total = 0;

    public SuccessFailure()
    {
    }

    public SuccessFailure(int success, int failure, int total)
    {
      Success = success;
      Failure = failure;
      Total = total;
    }

    public static SuccessFailure operator +(SuccessFailure c1, SuccessFailure c2)
    {
      return new SuccessFailure(c1.Success + c2.Success, c1.Failure + c2.Failure, c1.Total + c2.Total);
    }
  }

}
